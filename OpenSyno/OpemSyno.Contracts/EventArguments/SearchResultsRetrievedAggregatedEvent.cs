using System.Collections.Generic;

namespace OpenSyno.Services
{
    public class SearchResultsRetrievedAggregatedEvent
    {
        public IEnumerable<ISynoItem> Results { get; set; }

        public SearchResultsRetrievedAggregatedEvent(IEnumerable<ISynoItem> results)
        {
            Results = results;
        }
    }
}