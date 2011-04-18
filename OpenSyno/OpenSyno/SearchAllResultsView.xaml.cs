using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Ninject;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    public partial class SearchAllResultsView : PhoneApplicationPage
    {
        public SearchAllResultsView()
        {
            this.Loaded += PageLoaded;
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
            {
                string keyword = NavigationContext.QueryString["keyword"];
                DataContext = IoC.Container.Get<ISearchAllResultsViewModelFactory>().Create(keyword, new PageSwitchingService(NavigationService));                
            }
        }

        private void ApplicationBarPlayLast(object sender, EventArgs e)
        {
            var viewModel = (SearchAllResultsViewModel)DataContext;
            viewModel.PlayLastCommand.Execute(null);
        }

        private void ApplicationBarShowPlayQueue(object sender, EventArgs e)
        {
            var viewModel = (SearchAllResultsViewModel)DataContext;
            viewModel.ShowPlayQueueCommand.Execute(null);
        }
    }

    public interface ISearchAllResultsViewModelFactory
    {
        ISearchAllResultsViewModel Create(string keyword, IPageSwitchingService pageSwitchingService);
    }

    public class SearchAllResultsViewModelFactory : ISearchAllResultsViewModelFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private IEnumerable<SynoTrack> _lastResults;

        public SearchAllResultsViewModelFactory(IEventAggregator eventAggregator)
        {            
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<CompositePresentationEvent<TrackSearchResultsRetrievedAggregatedEvent>>().Subscribe(payload =>
                                                                                                                        {
                                                                                                                            _lastResults = payload.Results;
                                                                                                                        },
             true);
        }

        #region Implementation of ISearchAllResultsViewModelFactory

        public ISearchAllResultsViewModel Create(string keyword, IPageSwitchingService pageSwitchingService)
        {
            return new SearchAllResultsViewModel(_eventAggregator, pageSwitchingService, keyword, _lastResults);
        }

        #endregion
    }

    public interface ISearchAllResultsViewModel
    {
        string Keyword { get; set; }
        ObservableCollection<TrackViewModel> SearchResults { get; set; }
    }

    public class SearchAllResultsViewModel : ISearchAllResultsViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPageSwitchingService _pageSwitchingService;


        public SearchAllResultsViewModel(IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService, string keyword, IEnumerable<SynoTrack> lastResults)
        {
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;
            if (pageSwitchingService == null) throw new ArgumentNullException("pageSwitchingService");
            if (keyword == null) throw new ArgumentNullException("keyword");
            if (lastResults == null) throw new ArgumentNullException("lastResults");
            ShowPlayQueueCommand = new DelegateCommand(OnShowPlayQueue);
            PlayLastCommand = new DelegateCommand(OnPlayLast);
            Keyword = keyword;
            SearchResults = new ObservableCollection<TrackViewModel>();
            foreach (var lastResult in lastResults)
            {
                SearchResults.Add(new TrackViewModel(lastResult));
            }
        }

        private void OnPlayLast()
        {
            var tracksToPlay = from track in SearchResults where track.IsSelected select track;
            _eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Publish(new PlayListOperationAggregatedEvent(PlayListOperation.Append, tracksToPlay));
        }

        private void OnShowPlayQueue()
        {
            _pageSwitchingService.NavigateToPlayQueue();
        }

        public string Keyword { get; set; }
        public ObservableCollection<TrackViewModel> SearchResults { get; set; }

        public ICommand ShowPlayQueueCommand { get; set; }
        public ICommand PlayLastCommand { get; set; }
    }
}

