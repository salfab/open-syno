namespace OpenSyno.ViewModels
{
    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class SearchResultItemViewModelFactory : ISearchResultItemViewModelFactory
    {
        private IUrlParameterToObjectsPlateHeater _urlParameterToObjectsPlateHeater;

        public SearchResultItemViewModelFactory(IUrlParameterToObjectsPlateHeater urlParameterToObjectsPlateHeater)
        {
            this._urlParameterToObjectsPlateHeater = urlParameterToObjectsPlateHeater;
        }

        // Eventaggregator needs to be moved to .ctor
        public SearchResultItemViewModel Create(SynoItem result, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService)
        {
            return new SearchResultItemViewModel(result, eventAggregator, pageSwitchingService, _urlParameterToObjectsPlateHeater);
        }
    }
}