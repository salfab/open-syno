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
            string keyword = NavigationContext.QueryString["keyword"];

            DataContext = IoC.Container.Get<ISearchAllResultsViewModelFactory>().Create(keyword, new PageSwitchingService(NavigationService));
        }
    }

    public interface ISearchAllResultsViewModelFactory
    {
        ISearchAllResultsViewModel Create(string keyword, IPageSwitchingService pageSwitchingService);
    }

    public class SearchAllResultsViewModelFactory : ISearchAllResultsViewModelFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private IEnumerable<SynoItem> _lastResults;

        public SearchAllResultsViewModelFactory(IEventAggregator eventAggregator)
        {            
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<CompositePresentationEvent<SearchResultsRetrievedAggregatedEvent>>().Subscribe(payload =>
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
        ObservableCollection<SearchResultItemViewModel> SearchResults { get; set; }
    }

    public class SearchAllResultsViewModel : ISearchAllResultsViewModel
    {
        private readonly IEventAggregator _eventAggregator;

        public SearchAllResultsViewModel(IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService, string keyword, IEnumerable<SynoItem> lastResults)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (pageSwitchingService == null) throw new ArgumentNullException("pageSwitchingService");
            if (keyword == null) throw new ArgumentNullException("keyword");
            if (lastResults == null) throw new ArgumentNullException("lastResults");

            Keyword = keyword;
            SearchResults = new ObservableCollection<SearchResultItemViewModel>();
            foreach (var lastResult in lastResults)
            {
                SearchResults.Add(new SearchResultItemViewModel(lastResult, eventAggregator, pageSwitchingService));
            }
        }
        public string Keyword { get; set; }
        public ObservableCollection<SearchResultItemViewModel> SearchResults { get; set; }
    }
}

