namespace OpenSyno
{
    using System.Collections.Generic;

    public interface ISearchResultsViewModel
    {
        IEnumerable<SearchResultItemViewModel> SearchResults { get; set; }
    }
}