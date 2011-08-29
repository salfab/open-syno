using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using System.Windows;

    using OpenSyno.Services;
    using OpenSyno.ViewModels;

    public class ArtistPanoramaAlbumDetailItem : ArtistPanoramaItemViewModel
    {
        private const string TracksPropertyName = "Tracks";

        private readonly ISearchService _searchService;

        public ISynoItem AlbumItemInfo { get; set; }

        public ICommand SelectAllOrNoneCommand { get; set; }

        public ICommand PlayListOperationCommand { get; set; }

        private ObservableCollection<TrackViewModel> _tracks;
        private IEventAggregator _eventAggregator;

        private readonly INotificationService _notificationService;

        public ObservableCollection<TrackViewModel> Tracks
        {
            get
            {
                return _tracks;
            }
            set
            {
                _tracks = value;
                OnPropertyChanged(TracksPropertyName);
            }
        }

        public ArtistPanoramaAlbumDetailItem(ISynoItem album, ISearchService searchService, IEventAggregator eventAggregator, INotificationService notificationService) : base(ArtistPanoramaItemKind.AlbumDetail)
        {
            this._searchService = searchService;
            _eventAggregator = eventAggregator;
            _notificationService = notificationService;
            AlbumItemInfo = album;
            Header = album.Title;

            Tracks = new ObservableCollection<TrackViewModel>();

            IsBusy = true;
            
            // TODO : List tracks of the album.
            _searchService.GetTracksForAlbum(album, GetTracksForAlbumCompleted);

            SelectAllOrNoneCommand = new DelegateCommand(OnSelectAllOrNone);
            PlayListOperationCommand = new DelegateCommand(OnPlayListOperation);
        }

        private void OnPlayListOperation()
        {
            var operation = PlayListOperation.Append;
            IEnumerable<TrackViewModel> selectedItems = Tracks.Where(o => o.IsSelected);
            if (selectedItems.Count() < 1)
            {
                _notificationService.Warning("Please, select at least one track to play.", "No track selected");
            }
            else
            {
                _eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Publish(new PlayListOperationAggregatedEvent(operation, selectedItems));                
            }
        }

        private void OnSelectAllOrNone()
        {
            if (Tracks.Count > 0)
            {
                var newSelectionState = !Tracks.First().IsSelected;
                foreach (var track in Tracks)
                {
                    track.IsSelected = newSelectionState;
                }
            }
        }

        private void GetTracksForAlbumCompleted(IEnumerable<ISynoTrack> tracks, long l, ISynoItem arg3)
        {
            // We first want to populate a collection and then assign it to the bound property, otherwise, there will be just too many collection changed notiications.
            // we could also have disabled the binding and re-enabled, but hey, what's the gain ?
            var newTracks = new ObservableCollection<TrackViewModel>();
            foreach(var track in tracks.Select(o => new TrackViewModel(o)))
            {                
                newTracks.Add(track);
            }

            Tracks = newTracks;
            IsBusy = false;
        }
    }
}