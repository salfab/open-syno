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
using Ninject.Parameters;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    public partial class SearchAllResultsView : PhoneApplicationPage
    {
        private string SearchAllMusicResults = "AllMusicSearchResultsTicket";
        private bool _newPageInstance = false;
        private IEnumerable<SynoTrack> _searchResults;
        public SearchAllResultsView()
        {
            _newPageInstance = true;
            this.Loaded += PageLoaded;
            InitializeComponent();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            State[SearchAllMusicResults] = _searchResults.ToArray();
            base.OnNavigatedFrom(e);
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
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            // the page is an humble object, and the navigatorService, its sole dependency.
            var navigator = IoC.Container.Get<INavigatorService>();
            navigator.ActivateNavigationService(NavigationService, true);

            if (DataContext == null)
            {
                string keyword = NavigationContext.QueryString["keyword"];

                if (_newPageInstance && State.ContainsKey(SearchAllMusicResults))
                {
                    _searchResults = (IEnumerable<SynoTrack>)this.State[this.SearchAllMusicResults];
                }
                else
                {
                    _searchResults = (IEnumerable<SynoTrack>)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(SearchAllMusicResults);
                }

                DataContext = IoC.Container.Get<ISearchAllResultsViewModelFactory>().Create(keyword, _searchResults);
            }
        }
    }

    public interface ISearchAllResultsViewModelFactory
    {
        ISearchAllResultsViewModel Create(string keyword, IEnumerable<SynoTrack> searchResults);
    }

    public class SearchAllResultsViewModelFactory : ISearchAllResultsViewModelFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPageSwitchingService _pageSwitchingService;

        public SearchAllResultsViewModelFactory(IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService)
        {            
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;

        }

        #region Implementation of ISearchAllResultsViewModelFactory

        public ISearchAllResultsViewModel Create(string keyword, IEnumerable<SynoTrack> searchResults)
        {
            return new SearchAllResultsViewModel(_eventAggregator, _pageSwitchingService, keyword, searchResults);
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

        public string Keyword { get; set; }
        public ICommand PlayLastCommand { get; set; }
        public ObservableCollection<TrackViewModel> SearchResults { get; set; }
        public ICommand ShowPlayQueueCommand { get; set; }
        private void OnPlayLast()
        {
            var tracksToPlay = from track in SearchResults where track.IsSelected select track;
            _eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Publish(new PlayListOperationAggregatedEvent(PlayListOperation.Append, tracksToPlay));
        }

        private void OnShowPlayQueue()
        {
            _pageSwitchingService.NavigateToPlayQueue();
        }

    }
}

