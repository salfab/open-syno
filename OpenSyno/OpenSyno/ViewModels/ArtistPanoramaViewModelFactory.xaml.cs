using Microsoft.Practices.Prism.Events;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
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
            return new ArtistPanoramaViewModel(_searchService, _eventAggregator, _pageSwitchingService, artist, _notificationService);
        }
    }
}