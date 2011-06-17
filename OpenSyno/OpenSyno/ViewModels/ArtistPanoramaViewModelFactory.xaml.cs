using Microsoft.Practices.Prism.Events;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    public class ArtistPanoramaViewModelFactory
    {
        private readonly ISearchService _searchService;

        private readonly IEventAggregator _eventAggregator;

        private IPageSwitchingService _pageSwitchingService;

        private readonly INotificationService _notificationService;

        public ArtistPanoramaViewModelFactory(ISearchService searchService, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService, INotificationService notificationService)
        {
            _searchService = searchService;
            this._notificationService = notificationService;
            _pageSwitchingService = pageSwitchingService;
            
            _eventAggregator = eventAggregator;            
        }

        public ArtistPanoramaViewModel Create(SynoItem artist)
        {
            var artistPanoramaViewModel = new ArtistPanoramaViewModel(this._searchService, this._eventAggregator, this._pageSwitchingService, artist, this._notificationService);
            return artistPanoramaViewModel;
        }

        //public ArtistPanoramaViewModel Create(SynoItem artist, IEnumerable<SynoItem> artistItems)
        //{
        //    ArtistPanoramaViewModel artistPanoramaViewModel = new ArtistPanoramaViewModel(this._searchService, this._eventAggregator, this._pageSwitchingService, artist, this._notificationService);
        //    artistPanoramaViewModel.BuildArtistItems(artistItems);
        //    return artistPanoramaViewModel;
        //}
    }
}