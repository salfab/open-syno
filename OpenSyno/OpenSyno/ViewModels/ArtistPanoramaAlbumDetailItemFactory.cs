namespace OpenSyno.ViewModels
{
    using System.Collections.Generic;

    using Microsoft.Practices.Prism.Events;

    using OpemSyno.Contracts;

    using Synology.AudioStationApi;

    public class ArtistPanoramaAlbumDetailItemFactory : IArtistPanoramaAlbumDetailItemFactory
    {
        private readonly INotificationService _notificationService;

        private readonly IEventAggregator _eventAggregator;

        private readonly ISearchService _searchService;

        public ArtistPanoramaAlbumDetailItemFactory(INotificationService notificationService, IEventAggregator eventAggregator, ISearchService searchService)
        {
            _notificationService = notificationService;
            _eventAggregator = eventAggregator;
            _searchService = searchService;
        }

        public IArtistPanoramaAlbumDetailItem Create(SynoItem album, IEnumerable<IAlbumViewModel> albums, int activeItemIndex)
        {
            return new ArtistPanoramaAlbumDetailItem(album, _searchService, _eventAggregator, _notificationService);
        }
    }
}