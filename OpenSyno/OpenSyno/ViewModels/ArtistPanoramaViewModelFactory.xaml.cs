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
        private SynoItem _defaultArtist;
        private IPageSwitchingService _pageSwitchingService;

        public ArtistPanoramaViewModelFactory(ISearchService searchService, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService)
        {
            _searchService = searchService;
            _pageSwitchingService = pageSwitchingService;
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<CompositePresentationEvent<SelectedArtistChangedAggregatedEvent>>().Subscribe(o => _defaultArtist = o.Artist, true);
        }

        public ArtistPanoramaViewModel Create()
        {
            return new ArtistPanoramaViewModel(_searchService, _eventAggregator, _pageSwitchingService, _defaultArtist);
        }
    }
}