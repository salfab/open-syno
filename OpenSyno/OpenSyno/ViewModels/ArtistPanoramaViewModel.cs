using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Synology.AudioStationApi;

namespace OpenSyno.ViewModels
{
    using OpemSyno.Contracts;

    using OpenSyno.Services;

    public class ArtistPanoramaViewModel : ViewModelBase
    {
        private readonly ISearchService _searchService;
        private readonly IEventAggregator _eventAggregator;

        private readonly IPageSwitchingService _pageSwitchingService;

        public ObservableCollection<IAlbumViewModel> ArtistAlbums { get; set; }

        private string _artistName;
        private const string ArtistNamePropertyName = "ArtistName";
        private const string IsBusyPropertyName = "IsBusy";

        private const string CurrentArtistItemIndexPropertyName = "CurrentArtistItemIndex";

        private void OnPlayLast()
        {
            IAlbumViewModel albumViewModel = ArtistAlbums[CurrentArtistItemIndex];
            var tracksToPlay = albumViewModel.Tracks.Where(t => t.IsSelected);
            _eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Publish(new PlayListOperationAggregatedEvent(PlayListOperation.Append, tracksToPlay));                

        }

        private void OnShowPlayQueue()
        {
            _pageSwitchingService.NavigateToPlayQueue();
        }

        public int CurrentArtistItemIndex
        {
            get
            {
                return _currentArtistItemIndex;
            }

            set
            {
                _currentArtistItemIndex = value;
                OnPropertyChanged(CurrentArtistItemIndexPropertyName);
            }
        }

        private void UpdateBusyness(object sender, PropertyChangedEventArgs e)
        {
            // FIXME : Remove magic strings
            if (e.PropertyName == "IsBusy")
            {
                IsBusy = this.ArtistAlbums.Any(o => o.IsBusy);
            }
        }

        private bool _isBusy;

        private IPanoramaItemSwitchingService _panoramaItemSwitchingService;

        private int _currentArtistItemIndex;

        private readonly INotificationService _notificationService;

        private SynoItem _artist;

        public ArtistPanoramaViewModel(SynoItem artist, IEnumerable<IAlbumViewModel> albums, int activePanelIndex, ISearchService searchService, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService, INotificationService notificationService)
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

            if (artist == null)
            {
                throw new ArgumentNullException("artist");
            }
            if (albums == null)
            {
                throw new ArgumentNullException("albums");
            }

            PlayLastCommand = new DelegateCommand(OnPlayLast);
            PlayCommand = new DelegateCommand(OnPlay);
            CurrentArtistItemIndex = activePanelIndex;
            _searchService = searchService;
            this._notificationService = notificationService;

            // TODO : Use IoC or Factory or whatever, but something to be able to inject our own implementation
            _panoramaItemSwitchingService = new PanoramaItemSwitchingService();

            _panoramaItemSwitchingService.ActiveItemChangeRequested += (s, e) => CurrentArtistItemIndex = e.NewItemIndex;
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;
            this.ArtistAlbums = new ObservableCollection<IAlbumViewModel>();
            foreach (var albumViewModel in albums)
            {
                this.ArtistAlbums.Add(albumViewModel);
            }

            this.ArtistAlbums.CollectionChanged += StartMonitoringElements;
            foreach (var album in this.ArtistAlbums)
            {
                album.PropertyChanged += UpdateBusyness;
            }

            ShowPlayQueueCommand = new DelegateCommand(OnShowPlayQueue);
            _artist = artist;
            ArtistName = _artist.Title;
        }

        private void OnPlay()
        {
            IAlbumViewModel albumViewModel = ArtistAlbums[CurrentArtistItemIndex];
            var tracksToPlay = albumViewModel.Tracks.Where(t => t.IsSelected);
            _eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Publish(new PlayListOperationAggregatedEvent(PlayListOperation.ClearAndPlay, tracksToPlay));                
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged(IsBusyPropertyName);
            }
        }

        private void StartMonitoringElements(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IAlbumViewModel artistItem in e.NewItems)
                {
                    artistItem.PropertyChanged += UpdateBusyness;
                } 
            }

            if (e.OldItems != null)
            {
                foreach (IAlbumViewModel artistItem in e.OldItems)
                {
                    artistItem.PropertyChanged -= UpdateBusyness;
                }
            }
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

        public ICommand ShowPlayQueueCommand { get; set; }

        public ICommand PlayLastCommand { get; set; }

        public ICommand PlayCommand { get; private set; }

        public void BuildArtistItems(IEnumerable<SynoItem> albums)
        {
            this.ArtistAlbums.Clear();            

            
            // add the page for the list of albums.
            //var albumsListPanelViewModel = new ArtistPanoramaAlbumsListItemViewModel(albums, _artist, this._pageSwitchingService, this._panoramaItemSwitchingService);
            
            //this.ArtistAlbums.Add(albumsListPanelViewModel);

            // the "all albums" items
            var allmusic = albums.Where(o => o.ItemID.StartsWith("musiclib_music_artist"));

            foreach (var album in albums.Except(allmusic))
            {
                // Fixme : use a factory      
                
                var albumDetail = new AlbumViewModel(album);
                this.ArtistAlbums.Add(albumDetail);
            }

            foreach (var album in allmusic)
            {
                // Fixme : use a factory
                var albumDetail = new AlbumViewModel(album);
                this.ArtistAlbums.Add(albumDetail);
            }

        }

        public void QueryAndBuildArtistItems()
        {
            _searchService.GetAlbumsForArtist(_artist, (albums, b, c) =>
            {                
                BuildArtistItems(albums);
            });
        }
    }
}