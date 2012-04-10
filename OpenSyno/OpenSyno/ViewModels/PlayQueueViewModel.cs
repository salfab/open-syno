namespace OpenSyno.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Phone.BackgroundAudio;
    using Microsoft.Practices.Prism.Commands;
    using Microsoft.Practices.Prism.Events;

    using OpemSyno.Contracts;

    using OpenSyno.Common;
    using OpenSyno.Contracts.Domain;
    using OpenSyno.Services;

    using Synology.AudioStationApi;

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
        /// <param name="openSynoSettings"></param>
        public PlayQueueViewModel(IEventAggregator eventAggregator, IPlaybackService playbackService, INotificationService notificationService, IOpenSynoSettings openSynoSettings, ILogService logService, ITrackViewModelFactory trackViewModelFactory, IPageSwitchingService pageSwitchingService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }

            if (playbackService == null)
            {
                throw new ArgumentNullException("playbackService");
            }

            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }

            if (openSynoSettings == null)
            {
                throw new ArgumentNullException("openSynoSettings");
            }

            if (trackViewModelFactory == null)
            {
                throw new ArgumentNullException("trackViewModelFactory");
            }

            if (pageSwitchingService == null)
            {
                throw new ArgumentNullException("pageSwitchingService");
            }

            _trackViewModelFactory = trackViewModelFactory;

            RemoveTracksFromQueueCommand = new DelegateCommand<IEnumerable<object>>(OnRemoveTracksFromQueue);
            
            Action consecutiveAlbumsIdPatcher = () =>
            {
                var previousAlbumGuid = Guid.Empty;
                string previousAlbumId = null;
                foreach (var trackViewModel in this.PlayQueueItems)
                {
                    if (previousAlbumId != trackViewModel.TrackInfo.ItemPid)
                    {
                        previousAlbumId = trackViewModel.TrackInfo.ItemPid;
                        previousAlbumGuid = Guid.NewGuid();
                    }
                    trackViewModel.ConsecutiveAlbumIdentifier = previousAlbumGuid;
                }
            };
            
            _playbackService = playbackService;
            this.PlayQueueItems = new ObservableCollection<TrackViewModel>(playbackService.GetTracksInQueue().Select(o => _trackViewModelFactory.Create(o.Guid, o.Track, this._pageSwitchingService)));
            this.PlayQueueItems.CollectionChanged += (s, ea) =>
                                                         {
                                                             consecutiveAlbumsIdPatcher();
                                                         };
            consecutiveAlbumsIdPatcher();
            _playbackService.PlayqueueChanged += this.OnPlayqueueChanged;            
            
            // FIXME : using aggregated event is not a great idea here : we'd rather use a service : that would be cleaner and easier to debug !
            eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Subscribe(OnPlayListOperation, true);
            this._notificationService = notificationService;
            _openSynoSettings = openSynoSettings;
            _logService = logService;
            _pageSwitchingService = pageSwitchingService;
            _playbackService.TrackStarted += (o, e) =>
                                                 {
                                                     CurrentArtwork = new Uri(e.Track.AlbumArtUrl, UriKind.Absolute);
                                                     this.ActiveTrack = this._trackViewModelFactory.Create(e.Guid, e.Track, this._pageSwitchingService);
                                                 };

            _playbackService.BufferingProgressUpdated += (o, e) =>
                {
                    // throttle refresh through binding.
                    if (_lastBufferProgressUpdate.AddMilliseconds(500) < DateTime.Now || e.BytesLeft == 0)
                    {
                        _lastBufferProgressUpdate = DateTime.Now;
                        this.BufferedBytesCount = e.FileSize - e.BytesLeft;
                        this.CurrentFileSize = e.FileSize;
                        Debug.WriteLine("Download progress : " + (double)(e.FileSize - e.BytesLeft) / (double)e.FileSize * (double)100.0);
                    }
                };

            _playbackService.TrackCurrentPositionChanged += (o, e) =>
                {
                    CurrentPlaybackPercentComplete = e.PlaybackPercentComplete;;
                    CurrentTrackPosition = e.Position;
                };

            PlayCommand = new DelegateCommand<TrackViewModel>(o => OnPlay(o), track => track != null);
            PlayNextCommand = new DelegateCommand(OnPlayNext);
            PausePlaybackCommand = new DelegateCommand(OnPausePlayback);
            ResumePlaybackCommand = new DelegateCommand(OnResumePlayback);
            PlayPreviousCommand = new DelegateCommand(OnPlayPrevious);
            SavePlaylistCommand = new DelegateCommand<IEnumerable<TrackViewModel>>(OnSavePlaylist, t => this.CurrentPlaylist.Id == Guid.Empty);
            SelectAllAlbumTracksCommand = new DelegateCommand<Guid>(OnSelectAllAlbumTracks);

            LoadSavedPlaylists();
            LoadCurrentPlaylist();
        }

        private void LoadSavedPlaylists()
        {
            if (_openSynoSettings.Playlists == null)
            {
                this.Playlists = new ObservableCollection<Playlist>();
            }
            else
            {
                this.Playlists = new ObservableCollection<Playlist>(_openSynoSettings.Playlists);
            }

            var unsavedPlayqueueIsPresent = this._openSynoSettings.Playlists.Any(playlist => playlist.Id.Equals(Guid.Empty));
            if (!unsavedPlayqueueIsPresent)
            {
                var unsavedPlayqueue = new Playlist(Guid.Empty, "unsaved playqueue");
                this.Playlists.Add(unsavedPlayqueue);
                this.AddPlaylistToPersistedSettings(unsavedPlayqueue);
            }
            else
            {
                this.PlayqueueSanityCheck(this._openSynoSettings.Playlists);
            }
            
        }

        private void PlayqueueSanityCheck(List<Playlist> playlists)
        {
            if (playlists.Count(o => o.Id.Equals(Guid.Empty)) > 1)
            {
                throw new ArgumentOutOfRangeException("There should be only one unsaved playqueue.");
            }
        }

        private void LoadCurrentPlaylist()
        {
            Guid currentPlaylistId = _openSynoSettings.CurrentPlaylistGuid;

            // NOTE - if there is a bug and we have two matches, then we are going to fallback to an unsaved playlist.
            var currentPlaylist = this.Playlists.SingleOrDefault(o => o.Id == currentPlaylistId);

            // 2+ or no matching playlists at all : 
            if (currentPlaylist == null)
            {
                currentPlaylist = this.Playlists.Single(o => o.Id.Equals(Guid.Empty));
            }

            this.ClearItems();
            this.AppendItems(currentPlaylist.Tracks, matchingGuid => { });
            this.CurrentPlaylist = currentPlaylist;

        }

        private void OnSelectAllAlbumTracks(Guid consecutiveAlbumId)
        {
            var tracksFromAlbum = this.PlayQueueItems.Where(trackViewModel => trackViewModel.ConsecutiveAlbumIdentifier == consecutiveAlbumId);
            bool currentValue = tracksFromAlbum.First().IsSelected;
            foreach (var trackViewModel in tracksFromAlbum)
            {
                trackViewModel.IsSelected = currentValue;
            }
        }

        private void OnPlay(TrackViewModel trackViewModel)
        {
            if (trackViewModel == null)
            {
                // TODO : Should we replace this with a "Play first" ?
                _notificationService.Error("Please, select a track before hitting the play button.", "No track is selected");
                return;
            }
            OnPlay(trackViewModel.Guid);
        }

        private void OnPlayqueueChanged(object sender, PlayqueueChangedEventArgs e)
        {
            if (e.RemovedItems != null)
            {
                foreach (var oldItem in e.RemovedItems)
                {
                    var guid = oldItem.Guid;
                    this.PlayQueueItems.Remove(this.PlayQueueItems.Single(o => o.Guid == guid));
                }
            }

            if (e.AddedItems != null)
            {
                foreach (GuidToTrackMapping newItem in e.AddedItems)
                {
                    this.PlayQueueItems.Add(this._trackViewModelFactory.Create(newItem.Guid, newItem.Track, this._pageSwitchingService));
                }
            }

            if (this.CurrentPlaylist != null && this.CurrentPlaylist.Id != Guid.Empty)
            {
                var savedPlayList = this.CurrentPlaylist;
                var playlist = this.Playlists.Single(o => o.Id == Guid.Empty);
                playlist.Tracks.Clear();
                playlist.Tracks.AddRange(savedPlayList.Tracks);
                this.CurrentPlaylist = playlist;
            }


            // Hack : we want to make sure the converter Will be re-evaluated, so the easiest way is to trigger a propery changed.
            OnPropertyChanged(PlayQueueItemsPropertyName);
        }

        private void OnSavePlaylist(IEnumerable<TrackViewModel> tracks)
        {
            if (tracks == null)
            {
                throw new ArgumentNullException("tracks");
            }
            if (tracks.Count() == 0)
            {
                _notificationService.Warning("Please add tracks before saving them as a playlist.", "Empty playqueue");
            }

            Playlist playlist = new Playlist();
            foreach (var trackViewModel in tracks)
            {
                playlist.Tracks.Add(trackViewModel);
            }

            // TODO : let the user input the name.
            playlist.Name = tracks.First().TrackInfo.Artist + " ...";
            playlist.Id = Guid.NewGuid();
            AddPlaylistToPersistedSettings(playlist);

            // Note - we might face problems in the future when we edit a play list. Make sure the edits get propagated to the persistence as well.
            Playlists.Add(playlist);

            this.ClearItems();
            this.AppendItems(playlist.Tracks, matchingGuid => { });
            this.CurrentPlaylist = playlist;

        }

        private void AddPlaylistToPersistedSettings(Playlist playlist)
        {
            this._openSynoSettings.Playlists.Add(playlist);
            
            // Note - we should have used a dedicated service to handle data persistence.
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        private Playlist _currentPlaylist;

        public Playlist CurrentPlaylist
        {
            get
            {
                return this._currentPlaylist;
            }
            set
            {
                if (value != this._currentPlaylist)
                {
                    // If the playlist was not set before, we don't need to prepare the underlying services for the change : there are no changes, it's the first assignation.
                    if (this._currentPlaylist != null)
                    {
                        // prepare the underlying services with the caches and ascii mappings before we define the value as the current playlist.
                        // it is important to assign the value after because it is a business rule to make an unsaved playqueue out of a modified saved playlist.
                        // therefore, assigning the value anytime sooner would result in switching back to an unsaved playqueue !
                        //this.ClearItems();
                        //this.AppendItems(value.Tracks, matchingGuid => { });
                    }


                    // TODO : Needs to raise the PropertyChanged event.
                    this._currentPlaylist = value;
                    
                    this.OnPropertyChanged(CurrentPlaylistPropertyName);

                    // NOTE : beware the race conditions.
                    this.CurrentPlaylistIndex = this.Playlists.IndexOf(value);

                    PersistCurrentPlaylistSettings(this.CurrentPlaylist);
                    
                }                                
            }
        }

        private void PersistCurrentPlaylistSettings(Playlist currentPlaylist)
        {
            _logService.Trace("PlayQueueViewModel.PersistCurrentPlaylistSettings : Persisting playlist  " + currentPlaylist.Name + " (" + currentPlaylist.Id +")");
            _openSynoSettings.CurrentPlaylistGuid = currentPlaylist.Id;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public int CurrentPlaylistIndex
        {
            get
            {
                return _currentPlaylistIndex;
            }
            set
            {
                if (value != _currentPlaylistIndex)
                {
                    this._currentPlaylistIndex = value;
                    this.OnPropertyChanged(CurrentPlaylistIndexPropertyName);
                    this.ClearItems();                    
                    this.AppendItems(this._currentPlaylist.Tracks,guids => {});
                    this.CurrentPlaylist = this.Playlists[this._currentPlaylistIndex];
                }
            }
        }

        public ObservableCollection<Playlist> Playlists { get; set; }

        private void OnRemoveTracksFromQueue(IEnumerable<object> tracks)
        {
            // FIXME : It seems that the check list box in the PlayQueueView doesn't discard the checked items after clicking remove.

            //for (int i = this.PlayQueueItems.Count - 1; i >= 0; i--)
            //{
            //    if (this.PlayQueueItems[i].IsSelected)
            //    {
            //        this.PlayQueueItems.RemoveAt(i);
            //    }
            //}
            var tracksToRemove = this.PlayQueueItems.Where(o => o.IsSelected).Select(o => o.Guid);
            _playbackService.RemoveTracksFromQueue(tracksToRemove);

        }

        private void OnResumePlayback()
        {
            _playbackService.ResumePlayback();
        }

        private void OnPausePlayback()
        {
            _playbackService.PausePlayback();
        }

        private void OnPlayPrevious()
        {
            _playbackService.SkipPrevious();
        }

        /// <summary>
        /// Called when the Play next command is triggered.
        /// </summary>
        private void OnPlayNext()
        {
            SynoTrack nextSynoTrack;

            _playbackService.SkipNext();
            //// Don't crash if there is no active track : take the first one in the playlist.
            //// (shouldn't happen, though, unless no track has ever been added to the queue, in which case, the nextTrack will be null anyway...)
            //if (ActiveTrack == null)
            //{
            //    nextSynoTrack = _playbackService.PlayqueueItems.FirstOrDefault();
            //}
            //else
            //{
            //    nextSynoTrack = this._playbackService.GetNextTrack(this.ActiveTrack.TrackInfo);                
            //}

            //if (nextSynoTrack != null)
            //{
            //    var nextTrack = new TrackViewModel(nextSynoTrack);
            //    OnPlay(nextTrack);
            //}
            //else
            //{
            //    _notificationService.Warning("There is no next track to play : all tracks have been played.", "No more tracks to play");                                
            //}
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
                if (!double.IsInfinity(value) && !double.IsNaN(value))
                {
                    OnPropertyChanged(CurrentPlaybackPercentCompletePropertyName);
                }
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

        public ICommand SelectAllAlbumTracksCommand { get; set; }

        public double Volume
        {
            get
            {
                // FIXME : Here, we'd like to have a dependency on th audio renderer... the design is not optimal.
                return _playbackService.GetVolume();
            }
            set
            {
                // FIXME : Here, we'd like to have a dependency on th audio renderer... the design is not optimal.
                _playbackService.SetVolume(value);
            }
        }

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

        private const string SelectedTrackPropertyName = "SelectedTrack";
        private DateTime _lastBufferProgressUpdate;

        private readonly INotificationService _notificationService;

        private readonly IOpenSynoSettings _openSynoSettings;
        private readonly ILogService _logService;
        private readonly ITrackViewModelFactory _trackViewModelFactory;
        private readonly IPageSwitchingService _pageSwitchingService;

        private static string CurrentPlaylistPropertyName = "CurrentPlaylist";

        private int _currentPlaylistIndex;

        private const string CurrentPlaylistIndexPropertyName = "CurrentPlaylistIndex";

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

        public ICommand SavePlaylistCommand { get; set; }

        public ICommand PlayPreviousCommand { get; set; }

        public ICommand PlayNextCommand { get; set; }

        public ICommand PausePlaybackCommand { get; set; }
        
        public ICommand ResumePlaybackCommand { get; set; }

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

        public void WakeUpFromTombstone()
        {
            var currentTrack = this._playbackService.GetCurrentTrack();
            if (currentTrack != null)
            {
                this.ActiveTrack = this._trackViewModelFactory.Create(currentTrack.Guid, currentTrack.Track, this._pageSwitchingService);
            }
        }

        private void AppendItems(IEnumerable<TrackViewModel> items, Action<Dictionary<SynoTrack, Guid>> callback)
        {
            //if (!IsPopulatingCurrentPlaylist(items))
            //{
            //    var existingTemporaryPlayqueue = this.Playlists.SingleOrDefault(o => o.Id == Guid.Empty);
            //    if (existingTemporaryPlayqueue != null)
            //    {
            //        this.Playlists.Remove(existingTemporaryPlayqueue);
            //    }

            //    Playlist playQueue = new Playlist
            //        {
            //            Id = Guid.Empty,
            //            Name = "Unsaved playqueue",
            //            Tracks = this.CurrentPlaylist.Tracks 
            //        };

            //    this.CurrentPlaylist = playQueue;
            //}

            var tracks = items.Select(o => o.TrackInfo);

            // int insertPosition = _playbackService.GetTracksCountInQueue();
            int insertPosition = PlayQueueItems.Count();

            _playbackService.InsertTracksToQueue(tracks, insertPosition, callback);
            //foreach (var trackViewModel in items)
            //{
            //    PlayQueueItems.Add(trackViewModel);
            //}
        }

        private bool IsPopulatingCurrentPlaylist(IEnumerable<TrackViewModel> itemsToPopulatePlaylistWith)
        {
            var isPopulatingCurrentPlaylist = this.PlayQueueItems.Count == 0 && this.CurrentPlaylist.Tracks.SequenceEqual(itemsToPopulatePlaylistWith);
            return isPopulatingCurrentPlaylist;
        }

        private void OnPlay(Guid guidOfTrackToPlay)
        {
            if (guidOfTrackToPlay == Guid.Empty)
            {
                throw new ArgumentNullException("guidOfTrackToPlay", "The play command has been triggered without specifying a track to play.");
            }

            _playbackService.PlayTrackInQueue(guidOfTrackToPlay);
        }

        public ICommand RemoveTracksFromQueueCommand { get; set; }

        /// <summary>
        /// Called when a play list operation is requested.
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnPlayListOperation(PlayListOperationAggregatedEvent e)
        {
            TrackViewModel trackToPlay;

            switch (e.Operation)
            {
                case PlayListOperation.ClearAndPlay:
                    ClearItems();
                    
                    // test lines below before uncommenting
                    //_playbackService.ClearPlayQueue();
                    //_playbackService.InsertTracksToQueue(e.Items.Select(o => o.TrackInfo), 0);
                    if (e.Items.Count() > 0)
                    {
                        _logService.Trace(string.Format("PlayQueueViewModel.OnPlayListOperation : Appending {0} tracks", e.Items.Count()));
                        AppendItems(e.Items, matchingGeneratedGuids =>
                        {
                            trackToPlay = e.Items.First();
                            _logService.Trace(string.Format("Play queue cleared : Starting playback of track {0}", trackToPlay.TrackInfo.Title));
                            OnPlay(matchingGeneratedGuids[trackToPlay.TrackInfo]);
                        });
                    }
                    break;
                case PlayListOperation.InsertAfterCurrent:                    
                    break;
                case PlayListOperation.Append:
                    _logService.Trace(string.Format("PlayQueueViewModel.OnPlayListOperation : Appending {0} tracks", e.Items.Count()));
                    if (e.Items.Count() > 0)
                    {
                        AppendItems(e.Items, matchingGeneratedGuids =>
                                                 {
                                                     _logService.Trace(string.Format("Items appended : Current playback service status : {0}", _playbackService.Status));
                                                     if (_playbackService.Status != PlayState.Playing)
                                                     {
                                                         trackToPlay = e.Items.First();
                                                         _logService.Trace(string.Format("PlaybackStatus stopped - Starting playback of track {0}", trackToPlay.TrackInfo.Title));
                                                         OnPlay(matchingGeneratedGuids[trackToPlay.TrackInfo]);
                                                     }
                                                 });
                    }
                    

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearItems()
        {
            _playbackService.ClearTracksInQueue();
        }

        #endregion
    }
}
