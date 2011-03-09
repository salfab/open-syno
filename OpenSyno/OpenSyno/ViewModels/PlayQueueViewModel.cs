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
            _playbackService.TrackCurrentPositionChanged += (o, e) => CurrentTrackPosition = e.Position;

            PlayCommand = new DelegateCommand(OnPlay, () => ActiveTrack != null);
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

        private void OnPlay()
        {
            DefineNextActiveTrack();
            _playbackService.PlayTrackInQueue(this.ActiveTrack.TrackInfo);

            // TODO : Load image AFTER the mp3 is downloaded, or after it's started at least, to optimize the track download.
            CurrentArtwork = new Uri(ActiveTrack.TrackInfo.AlbumArtUrl, UriKind.Absolute);
        }

        private void DefineNextActiveTrack()
        {
            if (ActiveTrack == null)
            {
                ActiveTrack = PlayQueueItems.First();
            }
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
                        OnPlay();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
