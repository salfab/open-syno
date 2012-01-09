using System.Collections.Generic;

namespace OpenSyno.Services
{
    using Synology.AudioStationApi;

    public class SearchResultsRetrievedAggregatedEvent
    {
        public IEnumerable<SynoItem> Results { get; set; }

        public SearchResultsRetrievedAggregatedEvent(IEnumerable<SynoItem> results)
        {
            Results = results;
        }
    }
}