using System.Linq;
using Ninject;

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
        private const string IsAppLoadingPropertyName = "IsAppLoading";
        private readonly IEventAggregator _eventAggregator;
        private readonly ISignInService _signInService;

        private readonly IPageSwitchingService _pageSwitchingService;
        private readonly ISearchService _searchService;
        private bool _isAppLoading = false;
        private bool _isBusy;
        private string _searchPattern;

        public SearchViewModel(ISearchService searchService, IPageSwitchingService pageSwitchingService, IEventAggregator eventAggregator, ISignInService signInService)
        {
            _searchService = searchService;
            _pageSwitchingService = pageSwitchingService;
            _eventAggregator = eventAggregator;
            _signInService = signInService;
            
            // make sure the IsAppLoading is always up-to-date.
            signInService.SignInCompleted += (sender, ea) => IsAppLoading = ea.IsBusy;

            // just in case the event has previously been fired : we set its default value to the current value.
            IsAppLoading = _signInService.IsSigningIn;

            _eventAggregator.GetEvent<CompositePresentationEvent<SynoTokenReceivedAggregatedEvent>>().Subscribe(OnSynoTokenReceived, false);

            StartSearchCommand = new DelegateCommand<string>(OnStartSearch);
            StartSearchAllCommand = new DelegateCommand<string>(OnStartSearchAll);
            ShowAboutBoxCommand = new DelegateCommand(OnShowAboutBox);
        }

        /// <summary>
        /// Called when the syno token is received.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <remarks>This is the handler for the <see cref="SynoTokenReceivedAggregatedEvent"/> aggregated event.</remarks>
        public void OnSynoTokenReceived(SynoTokenReceivedAggregatedEvent payload)
        {
            IsAppLoading = false;
        }


        public ICommand StartSearchAllCommand { get; set; }

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

        public bool IsAppLoading
        {
            get
            {
                return _isAppLoading;
            }
            set
            {
                _isAppLoading = value;
                OnPropertyChanged(IsAppLoadingPropertyName);
            }
        }

        public ICommand ShowAboutBoxCommand { get; set; }

        private void OnShowAboutBox()
        {
            _pageSwitchingService.NavigateToAboutBox();
        }

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

        private void SearchAllCompleted(IEnumerable<SynoTrack> results, string keyword)
        {
            if (results == null) throw new ArgumentNullException("results");
            IsBusy = false;
            _pageSwitchingService.NavigateToSearchAllResults(keyword);
            _eventAggregator.GetEvent<CompositePresentationEvent<TrackSearchResultsRetrievedAggregatedEvent>>().Publish(new TrackSearchResultsRetrievedAggregatedEvent(results));
        }

        private void SearchCompleted(IEnumerable<SynoItem> results)
        {
            IsBusy = false;
            _pageSwitchingService.NavigateToSearchResults();
            _eventAggregator.GetEvent<CompositePresentationEvent<SearchResultsRetrievedAggregatedEvent>>().Publish(new SearchResultsRetrievedAggregatedEvent(results));
        }    
    }

    public class TrackSearchResultsRetrievedAggregatedEvent 
    {
        public IEnumerable<SynoTrack> Results { get; set; }

        public TrackSearchResultsRetrievedAggregatedEvent(IEnumerable<SynoTrack> results)
        {
            Results = results;
        }
    }
}