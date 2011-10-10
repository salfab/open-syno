namespace OpenSyno.ViewModels
{
    using System;

    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class SearchResultItemViewModel
    {
        private IEventAggregator _eventAggregator;
        private IPageSwitchingService _pageSwitchingService;

        private readonly IUrlParameterToObjectsPlateHeater _urlParameterToObjectsPlateHeater;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultItemViewModel"/> class.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="pageSwitchingService">The page switching service.</param>
        /// <param name="urlParameterToObjectsPlateHeater"></param>
        public SearchResultItemViewModel(SynoItem itemInfo, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService, IUrlParameterToObjectsPlateHeater urlParameterToObjectsPlateHeater)
        {
            if (itemInfo == null) throw new ArgumentNullException("itemInfo");
            ItemSelectedCommand = new DelegateCommand(OnItemSelected);
            ItemInfo = itemInfo;
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;
            _urlParameterToObjectsPlateHeater = urlParameterToObjectsPlateHeater;
        }

        private void OnItemSelected()
        {
            //var payload = new SelectedArtistChangedAggregatedEvent
            //{
            //    Artist = ItemInfo
            //};

            //// HACK : Since we can't pass the payload to the view by injection, we assume that the event is being subscribed by the view model behind the viewmodel, and that this will occur in a timely fashion for the navigation to show only the information related to the payload.
            //_eventAggregator.GetEvent<CompositePresentationEvent<SelectedArtistChangedAggregatedEvent>>().Publish(payload);
            _urlParameterToObjectsPlateHeater.RegisterObject(ItemInfo.ItemID, ItemInfo);
            //_pageSwitchingService.NavigateToArtistPanorama(ItemInfo.ItemID);
            _pageSwitchingService.NavigateToArtistDetailView(ItemInfo.ItemID);
        }

        public DelegateCommand ItemSelectedCommand { get; set; }

        public SynoItem ItemInfo { get; set; }
    }
}