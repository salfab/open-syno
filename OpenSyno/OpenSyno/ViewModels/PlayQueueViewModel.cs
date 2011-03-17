namespace OpenSyno.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.Events;

    using OpenSyno.Services;

    public class PlayQueueViewModel : ViewModelBase
    {
        #region Constants and Fields

        private const string ActiveTrackPropertyName = "ActiveTrack";

        private const string CurrentArtworkPropertyName = "CurrentArtwork";

        private const string PlayQueueItemsPropertyName = "PlayQueueItems";

        private readonly IPlaybackService _playbackService;



        private TrackViewModel _activeTrack;

        private Uri _currentArtwork;

        private ObservableCollection<TrackViewModel> _playQueueItems;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayQueueViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="playbackService">The playback service.</param>
        public PlayQueueViewModel(IEventAggregator eventAggregator, IPlaybackService playbackService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }

            if (playbackService == null)
            {
                throw new ArgumentNullException("playbackService");
            }


            PlayQueueItems = new ObservableCollection<TrackViewModel>();

            eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Subscribe(OnPlayListOperation, true);
            _playbackService = playbackService;
            _playbackService.BufferingProgressUpdated += (o, e) =>
                {                    
                    this.BufferedBytesCount = e.FileSize - e.BytesLeft;
                    this.CurrentFileSize = e.FileSize;
                    Debug.WriteLine("Download progress : " + (double)(e.FileSize - e.BytesLeft) / (double)e.FileSize * (double)100.0);
                };

            _playbackService.TrackCurrentPositionChanged += (o, e) =>
                {
                    CurrentPlaybackPercentComplete = e.PlaybackPercentComplete;;
                    CurrentTrackPosition = e.Position;
                };

            PlayCommand = new DelegateCommand<TrackViewModel>(OnPlay, track => track != null);
        }

        private double _currentPlaybackPercentComplete;

        public double CurrentPlaybackPercentComplete
        {
            get
            {
                return _currentPlaybackPercentComplete;
            }

            set
            {
                _currentPlaybackPercentComplete = value;
                OnPropertyChanged(CurrentPlaybackPercentCompletePropertyName);
            }
        }

        public long CurrentFileSize
        {
            get
            {
                return _currentFileSize;
            }
            set
            {
                _currentFileSize = value;
                this.OnPropertyChanged(CurrentFileSizePropertyName);
            }
        }

        #endregion

        #region Properties

        public TrackViewModel ActiveTrack
        {
            get { return _activeTrack; }
            set
            {
                _activeTrack = value;
                OnPropertyChanged(ActiveTrackPropertyName);
            }
        }

        public TrackViewModel SelectedTrack
        {
            get
            {
                return _selectedTrack;
            }

            set
            {
                _selectedTrack  = value;
                OnPropertyChanged(SelectedTrackPropertyName);
            }
        }



        private long _bufferedBytesCount;

        public long BufferedBytesCount
        {
            get
            {
                return this._bufferedBytesCount;
            }
            set
            {
                this._bufferedBytesCount = value;
                this.OnPropertyChanged(BufferBytesCountPropertyName);
            }
        }

        private TimeSpan _currentTrackPosition;

        private long _currentFileSize;

        private TrackViewModel _selectedTrack;

        private string SelectedTrackPropertyName = "SelectedTrack";

        private const string CurrentPlaybackPercentCompletePropertyName = "CurrentPlaybackPercentComplete";

        private const string BufferBytesCountPropertyName = "BufferedBytesCount";

        private const string CurrentFileSizePropertyName = "CurrentFileSize";

        private const string CurrentTrackPositionPropertyName = "CurrentTrackPosition";

        public TimeSpan CurrentTrackPosition
        {
            get
            {
                return _currentTrackPosition;
            }
            set
            {
                _currentTrackPosition = value;
                OnPropertyChanged(CurrentTrackPositionPropertyName);
            }
        }

        public Uri CurrentArtwork
        {
            get
            {
                return _currentArtwork;
            }

            set
            {
                _currentArtwork = value;
                OnPropertyChanged(CurrentArtworkPropertyName);
            }
        }
    
        public ICommand PlayCommand { get; set; }

        public ObservableCollection<TrackViewModel> PlayQueueItems
        {
            get
            {
                return _playQueueItems;
            }

            set
            {
                _playQueueItems = value;
                OnPropertyChanged(PlayQueueItemsPropertyName);
            }
        }

        #endregion

        #region Methods

        private void AppendItems(IEnumerable<TrackViewModel> items)
        {
            var tracks = items.Select(o=>o.TrackInfo);

            int insertPosition = _playbackService.PlayqueueItems.Count();

            _playbackService.InsertTracksToQueue(tracks, insertPosition);

            foreach (var trackViewModel in items)
            {
                PlayQueueItems.Add(trackViewModel);
            }
        }

        private void OnPlay(TrackViewModel trackViewModel)
        {
            if (trackViewModel == null)
            {
                throw new ArgumentNullException("trackViewModel", "The play command has been triggered without specifying a track to play.");
            }

            _playbackService.PlayTrackInQueue(trackViewModel.TrackInfo);
            _playbackService.TrackStarted += (sender, ea) =>
                                                 {
                                                     CurrentArtwork = new Uri(ea.Track.AlbumArtUrl, UriKind.Absolute);

                                                     // FIXME : Use a factory so we can mock the active track !
                                                     ActiveTrack = new TrackViewModel(ea.Track);
                                                 };
        }      

        /// <summary>
        /// Called when a play list operation is requested.
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnPlayListOperation(PlayListOperationAggregatedEvent e)
        {
            switch (e.Operation)
            {
                case PlayListOperation.ClearAndPlay:
                    break;
                case PlayListOperation.InsertAfterCurrent:                    
                    break;
                case PlayListOperation.Append:
                    AppendItems(e.Items);
                    if (_playbackService.Status == PlaybackStatus.Stopped)
                    {
                        var trackToPlay = SelectedTrack != null ? SelectedTrack : PlayQueueItems.First();
                        OnPlay(trackToPlay);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
