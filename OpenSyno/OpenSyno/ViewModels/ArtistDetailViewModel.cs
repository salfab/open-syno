using System.Collections.Generic;

namespace OpenSyno.ViewModels
{
    using System.Collections.ObjectModel;

    using OpemSyno.Contracts;
    using OpemSyno.Contracts.Domain;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class ArtistDetailViewModel : ViewModelBase, IArtistDetailViewModel
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

        private ObservableCollection<IAlbumViewModel> _albums;
        private readonly ISearchService _searchService;

        private IAlbumViewModelFactory _albumViewModelFactory;

        private INavigatorService _navigatorSevice;

        private IPageSwitchingService _pageSwitchingService;

        public ArtistDetailViewModel(SynoItem artist, ISearchService searchService, IAlbumViewModelFactory albumViewModelFactory, INavigatorService navigatorSevice, IPageSwitchingService pageSwitchingService)
        {
            this.Albums = new ObservableCollection<IAlbumViewModel>();
            this.ArtistName = artist.Title;
            this.SimilarArtists = new ObservableCollection<IArtistViewModel>();
            this._searchService = searchService;
            _albumViewModelFactory = albumViewModelFactory;
            _navigatorSevice = navigatorSevice;
            _pageSwitchingService = pageSwitchingService;
            GetAlbumsAsync(artist);
            GetSimilarArtistsAsync(artist);
        }

        private void GetSimilarArtistsAsync(SynoItem artist)
        {
            // TODO : Look on last.fm
        }

        private void GetAlbumsAsync(SynoItem artist)
        {
            // TODO : request albums.    
         
            _searchService.GetAlbumsForArtist(artist, (a, b, c) =>
                                                          {
                                                              var albumsList = a;
                                                              foreach (var album in albumsList)
                                                              {
                                                                  IAlbumViewModel albumViewModel = _albumViewModelFactory.Create(album);
                                                                  // TODO : Register Selected event and on event handler : Load the album's tracks and navigate to its index in the artist's albums panorama.
                                                                  // Note : So far, the viewmodels are never removed from the list : that means we don't need to unregister that event. If this changes, 
                                                                  // then it will be necessary to enregister the event for each view model removed from the collection.
                                                                  albumViewModel.Selected += (s, e) =>
                                                                      {
                                                                          string albumId = ((AlbumViewModel)s).Album.ItemID;
                                                                          _navigatorSevice.UrlParameterToObjectsPlateHeater.RegisterObject(albumId, ((AlbumViewModel)s).Album);                                                                          
                                                                          _navigatorSevice.UrlParameterToObjectsPlateHeater.RegisterObject(artist.ItemID, artist);                                                                          
                                                                          _pageSwitchingService.NavigateToArtistPanorama(artist.ItemID, albumId);
                                                                          
                                                                      };
                                                                  Albums.Add(albumViewModel);
                                                              }
                                                          });
        }

        public ObservableCollection<IAlbumViewModel> Albums
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