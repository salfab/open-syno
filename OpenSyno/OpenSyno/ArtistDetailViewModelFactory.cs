using System;

using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using OpenSyno.Services;

    public class ArtistDetailViewModelFactory
    {
        private readonly ISearchService _searchService;

        private AlbumViewModelFactory _albumViewModelFactory;

        private readonly INavigatorService _navigatorSevice;

        private readonly IPageSwitchingService _pageSwitchingService;
        private readonly ITrackViewModelFactory _session;

        public ArtistDetailViewModelFactory(ISearchService searchService, AlbumViewModelFactory albumViewModelFactory, INavigatorService navigatorSevice, IPageSwitchingService pageSwitchingService, ITrackViewModelFactory session)
        {
            if (session == null) throw new ArgumentNullException("session");
            _searchService = searchService;
            _albumViewModelFactory = albumViewModelFactory;
            this._navigatorSevice = navigatorSevice;
            this._pageSwitchingService = pageSwitchingService;
            this._session = session;
        }

        public ArtistDetailViewModel Create(SynoItem artist)
        {
            return new ArtistDetailViewModel(artist, _searchService, _albumViewModelFactory,this._navigatorSevice, this._pageSwitchingService, this._session);
        }
    }
}