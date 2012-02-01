using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Seesharp.LastFmApi.Mango;

namespace OpenSyno.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using OpemSyno.Contracts.Domain;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class ArtistDetailViewModel : ViewModelBase
    {
        private string _artistName;

        // FXME : Use the property name helper to get it from a lambda expression instead of a magic string.
        private const string ArtistNamePropertyName = "ArtistName";
        private const string AlbumsPropertyName = "Albums";

        public string ArtistName
        {
            get
            {
                return this._artistName;
            }
            set
            {
                if (this._artistName != value)
                {
                    this._artistName = value;                   
                    this.OnPropertyChanged(ArtistNamePropertyName);
                }
            }
        }

        private ObservableCollection<AlbumViewModel> _albums;
        private readonly ISearchService _searchService;

        private AlbumViewModelFactory _albumViewModelFactory;

        private INavigatorService _navigatorSevice;

        private IPageSwitchingService _pageSwitchingService;
        private readonly ITrackViewModelFactory _trackViewModelFactory;

        public ArtistDetailViewModel(SynoItem artist, ISearchService searchService, AlbumViewModelFactory albumViewModelFactory, INavigatorService navigatorSevice, IPageSwitchingService pageSwitchingService, ITrackViewModelFactory trackViewModelFactory)
        {
            if (trackViewModelFactory == null) throw new ArgumentNullException("trackViewModelFactory");
            this.Albums = new ObservableCollection<AlbumViewModel>();
            this.ArtistName = artist.Title;
            this.SimilarArtists = new ObservableCollection<IArtistViewModel>();
            this._searchService = searchService;
            _albumViewModelFactory = albumViewModelFactory;
            _navigatorSevice = navigatorSevice;
            _pageSwitchingService = pageSwitchingService;
            _trackViewModelFactory = trackViewModelFactory;
            this.PopulateAlbumsAsync(artist);
            GetSimilarArtistsAsync(artist);
        }

        private void GetSimilarArtistsAsync(SynoItem artist)
        {
            // TODO : Look on last.fm
            LastFmApi api = new LastFmApi();
            var t = api.GetSimilarArtistsAsync(artist.Title);

            t.ContinueWith(o =>
                               {
                                   SimilarArtists.Clear();
                                   foreach (var lastFmArtist in o.Result)
                                   {
                                       // FIXME : USe a factory.
                                       SimilarArtists.Add(new ArtistViewModel(lastFmArtist.Name, lastFmArtist.Mbid, lastFmArtist.Url, lastFmArtist.Match, lastFmArtist.Images));
                                   }
                               }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void PopulateAlbumsAsync(SynoItem artist)
        {
            // TODO : request albums.    
         
            _searchService.GetAlbumsForArtist(artist, GetAlbumsForArtistCallback);
        }

        private void GetAlbumsForArtistCallback(IEnumerable<SynoItem> albums, long totalAlbumsCount, SynoItem artist)
        {
            foreach (var album in albums)
            {
                AlbumViewModel albumViewModel = _albumViewModelFactory.Create(album);

                // prepare an empty track list
                albumViewModel.Tracks.Clear();
                
                // album is busy because its tracks are getting loaded.
                albumViewModel.IsBusy = true;

                _searchService.GetTracksForAlbum(album,(synoTracks,count, containingAlbum) =>
                    {
                        // Note - would it work, based on the synoitem instead of the ItemID ?
                        // Note - we rely on the fact that there each album is only present once in the artists discography ( based on its ItemID ) otherwise, it crashes !
                        var currentAlbumViewModel = Albums.Single(a => a.Album.ItemID == containingAlbum.ItemID);
                        currentAlbumViewModel.Tracks.Clear();
                        foreach (var track in synoTracks)
                        {
                            // track guid is empty for now : it will be filled when the track gets added to the playqueue
                            currentAlbumViewModel.Tracks.Add(this._trackViewModelFactory.Create(Guid.Empty, track, this._pageSwitchingService));
                        }
                        currentAlbumViewModel.IsBusy = false;

                    });

                // TODO : Register Selected event and on event handler : Load the album's tracks and navigate to its index in the artist's albums panorama.
                // Note : So far, the viewmodels are never removed from the list : that means we don't need to unregister that event. If this changes, 
                // then it will be necessary to enregister the event for each view model removed from the collection.
                albumViewModel.Selected += (s, e) =>
                {
                    string albumId = ((AlbumViewModel)s).Album.ItemID;
                    _navigatorSevice.UrlParameterToObjectsPlateHeater.RegisterObject(albumId, s);
                    _navigatorSevice.UrlParameterToObjectsPlateHeater.RegisterObject(artist.ItemID, artist);
                    Guid albumsListTicket = Guid.NewGuid();
                    _navigatorSevice.UrlParameterToObjectsPlateHeater.RegisterObject(albumsListTicket.ToString(), this.Albums);
                    _pageSwitchingService.NavigateToArtistPanorama(artist.ItemID, albumId, albumsListTicket.ToString());

                };
                Albums.Add(albumViewModel);
            }
        }

        public ObservableCollection<AlbumViewModel> Albums
        {
            get
            {
                return this._albums;
            }
            set
            {
                if (this._albums != value)
                {
                    this._albums = value;
                    this.OnPropertyChanged(AlbumsPropertyName);                    
                }
            }
        }

        public ObservableCollection<IArtistViewModel> SimilarArtists { get; set; }
    }
}