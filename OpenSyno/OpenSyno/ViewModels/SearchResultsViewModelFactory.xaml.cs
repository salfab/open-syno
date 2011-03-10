namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class SearchResultsViewModelFactory
    {
        private IEnumerable<SynoItem> _lastSearchResults;
        private IEventAggregator _eventAggregator;

        public SearchResultsViewModelFactory(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            _eventAggregator = eventAggregator;
            _lastSearchResults = new List<SynoItem>();
            _eventAggregator.GetEvent<CompositePresentationEvent<SearchResultsRetrievedAggregatedEvent>>().Subscribe(SearchResultsUpdated, true);
        }

        private void SearchResultsUpdated(SearchResultsRetrievedAggregatedEvent payload)
        {
            _lastSearchResults = payload.Results;
        }

        public ISearchResultsViewModel Create(PageSwitchingService pageSwitchingService)
        {
            return new SearchResultsViewModel(pageSwitchingService, _lastSearchResults);
        }
    }
}