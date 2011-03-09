using System.Collections.Generic;
using Synology.AudioStationApi;

namespace OpenSyno.Services
{
    public class SearchResultsRetrievedAggregatedEvent
    {
        public IEnumerable<SynoItem> Results { get; set; }

        public SearchResultsRetrievedAggregatedEvent(IEnumerable<SynoItem> results)
        {
            Results = results;
        }
    }
}