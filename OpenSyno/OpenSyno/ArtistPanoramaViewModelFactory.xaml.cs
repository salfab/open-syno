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

        public ArtistPanoramaViewModelFactory(ISearchService searchService, IEventAggregator eventAggregator)
        {
            _searchService = searchService;
            _eventAggregator = eventAggregator;
            eventAggregator.GetEvent<CompositePresentationEvent<SelectedArtistChangedAggregatedEvent>>().Subscribe(o => _defaultArtist = o.Artist, true);
        }

        public ArtistPanoramaViewModel Create(PageSwitchingService pageSwitchingService)
        {
            return new ArtistPanoramaViewModel(_searchService, _eventAggregator, pageSwitchingService, _defaultArtist);
        }
    }
}