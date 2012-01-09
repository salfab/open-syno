namespace OpenSyno.ViewModels
{
    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public interface ISearchResultItemViewModelFactory
    {
        SearchResultItemViewModel Create(SynoItem result, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService);
    }
}