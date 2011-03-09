using System;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using OpenSyno.Services;
using Synology.AudioStationApi;

namespace OpenSyno
{
    public class SearchResultItemViewModel
    {
        private IEventAggregator _eventAggregator;
        private IPageSwitchingService _pageSwitchingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultItemViewModel"/> class.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="pageSwitchingService">The page switching service.</param>
        public SearchResultItemViewModel(SynoItem itemInfo, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService)
        {
            if (itemInfo == null) throw new ArgumentNullException("itemInfo");
            ItemSelectedCommand = new DelegateCommand(OnItemSelected);
            ItemInfo = itemInfo;
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;
        }

        private void OnItemSelected()
        {
            var payload = new SelectedArtistChangedAggregatedEvent
            {
                Artist = ItemInfo
            };
            _eventAggregator.GetEvent<CompositePresentationEvent<SelectedArtistChangedAggregatedEvent>>().Publish(payload);
            _pageSwitchingService.NavigateToArtistPanorama();
        }

        public DelegateCommand ItemSelectedCommand { get; set; }

        public SynoItem ItemInfo { get; set; }
    }
}