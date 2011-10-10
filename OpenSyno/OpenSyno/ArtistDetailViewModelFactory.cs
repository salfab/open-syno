using OpemSyno.Contracts;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    public class ArtistDetailViewModelFactory : IArtistDetailViewModelFactory
    {
        private readonly ISearchService _searchService;

        public ArtistDetailViewModelFactory(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public IArtistDetailViewModel Create(SynoItem artist)
        {
            return new ArtistDetailViewModel(artist, _searchService);
        }
    }
}