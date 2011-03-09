using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Events;
using Synology.AudioStationApi;

namespace OpenSyno.ViewModels
{
    using System.Windows.Navigation;

    using OpenSyno.Services;

    public class ArtistPanoramaViewModel : ViewModelBase
    {
        private readonly ISearchService _searchService;
        private readonly IEventAggregator _eventAggregator;

        private readonly PageSwitchingService _pageSwitchingService;

        public ObservableCollection<ArtistPanoramaItem> PanoramaItems { get; set; }

        private string _artistName;
        private const string  ArtistNamePropertyName = "ArtistName";

        public ArtistPanoramaItemKind PanoramaItemKind { get; set; }

        public ArtistPanoramaViewModel(ISearchService searchService, IEventAggregator eventAggregator, PageSwitchingService pageSwitchingService, SynoItem artist)
        {
            if (searchService == null)
            {
                throw new ArgumentNullException("searchService");
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }

            if (pageSwitchingService == null)
            {
                throw new ArgumentNullException("pageSwitchingService");
            }
            if (artist == null) throw new ArgumentNullException("artist");

            
            _searchService = searchService;
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;
            PanoramaItems = new ObservableCollection<ArtistPanoramaItem>();
            eventAggregator.GetEvent<CompositePresentationEvent<SelectedArtistChangedAggregatedEvent>>().Subscribe(OnSelectedArtistChanged, true);
            LoadArtistInfo(artist);

        }

        private void OnSelectedArtistChanged(SelectedArtistChangedAggregatedEvent ea)
        {
            LoadArtistInfo(ea.Artist);
        }

        private void LoadArtistInfo(SynoItem artist)
        {
            ArtistName = artist.Title;
            GetAlbumsForArtist(artist);
        }

        public string ArtistName
        {
            get
            {
                return _artistName;
            }
            set 
            {
                _artistName = value;
                OnPropertyChanged(ArtistNamePropertyName);
            }
        }

        private void GetAlbumsForArtist(SynoItem artist)
        {
            _searchService.GetAlbumsForArtist(artist, GetAlbumsForArtistCompleted);
        }

        private void GetAlbumsForArtistCompleted(IEnumerable<SynoItem> albums, long total, SynoItem artist)
        {
            // make sure the old items are cleared.
            PanoramaItems.Clear();

            // add the page for the list of albums.
            var albumsListPanel = new ArtistPanoramaAlbumsListItem(albums, artist, _pageSwitchingService);

            PanoramaItems.Add(albumsListPanel);

            foreach (var album in albums)
            {
                var albumDetail = new ArtistPanoramaAlbumDetailItem(album, _searchService, _eventAggregator);                
                PanoramaItems.Add(albumDetail);

            }
        }
    }
}