using System.Collections.Generic;

namespace OpenSyno.ViewModels
{
    using System.Collections.ObjectModel;

    using OpemSyno.Contracts;
    using OpemSyno.Contracts.Domain;

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

        private ObservableCollection<SynoItem> _albums;
        private readonly ISearchService _searchService;

        public ArtistDetailViewModel(SynoItem artist, ISearchService searchService)
        {
            this.Albums = new ObservableCollection<SynoItem>();
            this.ArtistName = artist.Title;
            this.SimilarArtists = new ObservableCollection<IArtistViewModel>();
            this._searchService = searchService;
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
                                                                  Albums.Add(album);
                                                              }
                                                          });
        }

        public ObservableCollection<SynoItem> Albums
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