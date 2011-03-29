namespace OpenSyno.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class SearchViewModel : ViewModelBase
    {


        private const string SearchPatternPropertyName = "SearchPattern";
        private const string IsBusyPropertyName = "IsBusy";
        private readonly IEventAggregator _eventAggregator;

        private readonly IPageSwitchingService _pageSwitchingService;
        private readonly ISearchService _searchService;
        private bool _isBusy;
        private string _searchPattern;

        public SearchViewModel(ISearchService searchService, IPageSwitchingService pageSwitchingService, IEventAggregator eventAggregator)
        {
            _searchService = searchService;
            _pageSwitchingService = pageSwitchingService;
            _eventAggregator = eventAggregator;

            StartSearchCommand = new DelegateCommand<string>(OnStartSearch);
            StartSearchAllCommand = new DelegateCommand<string>(OnStartSearchAll);
            ShowAboutBoxCommand = new DelegateCommand(OnShowAboutBox);
        }



        public ICommand StartSearchAllCommand { get; set; }

        private void OnShowAboutBox()
        {
            _pageSwitchingService.NavigateToAboutBox();
        }

        public ICommand StartSearchCommand { get; set; }

        //protected string SearchPattern
        //{
        //    get
        //    {
        //        return _searchPattern;
        //    }
        //    set
        //    {
        //        _searchPattern = value;
        //        OnPropertyChanged(SearchPatternPropertyName);
        //    }
        //}

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged(IsBusyPropertyName);
            }
        }

        public ICommand ShowAboutBoxCommand { get; set; }

        /// <summary>
        /// Called when the search gets started.
        /// </summary>
        private void OnStartSearch(string keyword)
        {
            var isSearchIssued = _searchService.SearchArtists(keyword, SearchCompleted);
            IsBusy = isSearchIssued;
        }

        private void OnStartSearchAll(string keyword)
        {
            var isSearchIssued = _searchService.SearchAllMusic(keyword, SearchAllCompleted);
            IsBusy = isSearchIssued;
        }

        private void SearchAllCompleted(IEnumerable<SynoTrack> results)
        {
            var list = new List<SynoTrack>(results);
            throw new NotImplementedException();
        }

        private void SearchCompleted(IEnumerable<SynoItem> results)
        {
            IsBusy = false;
            _pageSwitchingService.NavigateToSearchResults();
            _eventAggregator.GetEvent<CompositePresentationEvent<SearchResultsRetrievedAggregatedEvent>>().Publish(new SearchResultsRetrievedAggregatedEvent(results));
        }

        private void GetAllArtistsCompleted(IEnumerable<SynoItem> results)
        {
            _eventAggregator.GetEvent<CompositePresentationEvent<SearchResultsRetrievedAggregatedEvent>>().Publish(new SearchResultsRetrievedAggregatedEvent(results));
            _pageSwitchingService.NavigateToSearchResults();

        }
    }
}