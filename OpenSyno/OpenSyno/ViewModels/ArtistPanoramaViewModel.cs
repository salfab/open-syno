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
    using System.Windows.Navigation;

    using OpenSyno.Services;

    public class ArtistPanoramaViewModel : ViewModelBase
    {
        private readonly ISearchService _searchService;
        private readonly IEventAggregator _eventAggregator;

        private readonly IPageSwitchingService _pageSwitchingService;

        public ObservableCollection<ArtistPanoramaItemViewModel> ArtistItems { get; set; }

        private string _artistName;
        private const string ArtistNamePropertyName = "ArtistName";
        private const string IsBusyPropertyName = "IsBusy";

        private const string CurrentArtistItemIndexPropertyName = "CurrentArtistItemIndex";

        public ArtistPanoramaItemKind PanoramaItemKind { get; set; }

        public ArtistPanoramaViewModel(ISearchService searchService, IEventAggregator eventAggregator, IPageSwitchingService pageSwitchingService, SynoItem artist, INotificationService notificationService)
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

            _searchService = searchService;
            this._notificationService = notificationService;

            // TODO : Use IoC or Factory or whatever, but something to be able to inject our own implementation
            _panoramaItemSwitchingService = new PanoramaItemSwitchingService();

            _panoramaItemSwitchingService.ActiveItemChangeRequested += (s, e) => CurrentArtistItemIndex = e.NewItemIndex;
            _eventAggregator = eventAggregator;
            _pageSwitchingService = pageSwitchingService;
            ArtistItems = new ObservableCollection<ArtistPanoramaItemViewModel>();
            ArtistItems.CollectionChanged += StartMonitoringElements;
            foreach (ArtistPanoramaItemViewModel artistItem in ArtistItems)
            {
                artistItem.PropertyChanged += UpdateBusyness;
            }

            ShowPlayQueueCommand = new DelegateCommand(OnShowPlayQueue);
            _artist = artist;
            ArtistName = _artist.Title;
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
            if (e.PropertyName == ArtistPanoramaItemViewModel.IsBusyPropertyName)
            {
                IsBusy = ArtistItems.Any(o => o.IsBusy);
            }
        }

        private bool _isBusy;

        private IPanoramaItemSwitchingService _panoramaItemSwitchingService;

        private int _currentArtistItemIndex;

        private readonly INotificationService _notificationService;

        private SynoItem _artist;

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
                foreach (ArtistPanoramaItemViewModel artistItem in e.NewItems)
                {
                    artistItem.PropertyChanged += UpdateBusyness;
                } 
            }

            if (e.OldItems != null)
            {
                foreach (ArtistPanoramaItemViewModel artistItem in e.OldItems)
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

        public void BuildArtistItems(IEnumerable<ISynoItem> albums)
        {
            this.ArtistItems.Clear();            

            // add the page for the list of albums.
            var albumsListPanelViewModel = new ArtistPanoramaAlbumsListItemViewModel(albums, _artist, this._pageSwitchingService, this._panoramaItemSwitchingService);
            
            this.ArtistItems.Add(albumsListPanelViewModel);

            // the "all albums" items
            var allmusic = albums.Where(o => o.ItemID.StartsWith("musiclib_music_artist"));

            foreach (var album in albums.Except(allmusic))
            {
                // Fixme : use a factory                
                var albumDetail = new ArtistPanoramaAlbumDetailItem(album, this._searchService, this._eventAggregator, this._notificationService);                
                this.ArtistItems.Add(albumDetail);
            }

            foreach (var album in allmusic)
            {
                // Fixme : use a factory
                var albumDetail = new ArtistPanoramaAlbumDetailItem(album, this._searchService, this._eventAggregator, this._notificationService);
                this.ArtistItems.Add(albumDetail);
            }

        }
    }
}