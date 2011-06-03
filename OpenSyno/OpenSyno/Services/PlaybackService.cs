using Microsoft.Phone.Shell;
using Ninject;
using OpenSyno.Helpers;

namespace OpenSyno.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using Synology.AudioStationApi;

    public class PlaybackService : IPlaybackService
    {
        /// <summary>
        /// The service responsible for downloading and rendering the audio files.
        /// </summary>
        private readonly IAudioRenderingService _audioRenderingService;

        private PlaybackStatus _status;

        // HACK : remove this and use the duration of the media stream source
        /// <summary>
        /// the last started track
        /// </summary>
        /// <remarks>The MP3 Media Stream Source is buggy with Variable bitrates, so the duration is not computed correctly. Instead, we want to use the duration exposed by Synology. In order to do this, we need to keep a reference on the last played SynoTrack.</remarks>
        private SynoTrack _lastStartedTrack;

        private ILogService _logService;

        /// <summary>
        /// Gets or sets what strategy should be used to define the next track to play.
        /// </summary>
        /// <value>The playback continuity.</value>
        public PlaybackContinuity PlaybackContinuity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the next track to be played should be preloaded.
        /// </summary>
        /// <value><c>true</c> if the next track to be played should be preloaded; otherwise, <c>false</c>.</value>
        public bool PreloadTracks { get; set; }


        /// <summary>
        /// Gets the items in the playqueue.
        /// </summary>
        /// <value>The items in the playqueue.</value>
        public IList<SynoTrack> PlayqueueItems { get; private set; }

        public PlaybackStatus Status
        {
            get { return _status; }            
        }

        /// <summary>
        /// Clears the play queue.
        /// </summary>
        public void ClearPlayQueue()
        {
            PlayqueueItems.Clear();
        }

        /// <summary>
        /// Inserts the specified tracks to the play queue.
        /// </summary>
        /// <param name="tracks">The tracks.</param>
        /// <param name="insertPosition">The position in the play queue where to insert the specified tracks.</param>
        public void InsertTracksToQueue(IEnumerable<SynoTrack> tracks, int insertPosition)
        {
            foreach (var synoTrack in tracks)
            {
                PlayqueueItems.Insert(insertPosition, synoTrack);
                insertPosition++;
            }
        }

        /// <summary>
        /// Plays the specified track. It must be present in the queue.
        /// </summary>
        /// <param name="trackToPlay">The track to play.</param>
        public void PlayTrackInQueue(SynoTrack trackToPlay)
        {            
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            // var index = PlayqueueItems.IndexOf(trackToPlay);
            _audioRenderingService.Bufferize(BufferizedCallback, BufferingProgressChanged, trackToPlay);
            //var client = new WebClient();
            //client.OpenReadCompleted += (o, ea) =>
            //    _audioRenderingService.Play(ea.Result);
            //client.DownloadProgressChanged += (o, ea) =>
            //    {
            //        BufferingProgressUpdatedEventArgs bufferingProgressUpdatedEventArgs = new BufferingProgressUpdatedEventArgs { BytesLeft = ea.TotalBytesToReceive - ea.BytesReceived, FileName = string.Empty, FileSize = ea.TotalBytesToReceive };
            //        OnBufferingProgressUpdated(bufferingProgressUpdatedEventArgs);                    
            //    };
            //client.OpenReadAsync(new Uri(trackToPlay.Res));
            _status = PlaybackStatus.Buffering;
        }

        protected void OnTrackStarted(TrackStartedEventArgs trackStartedEventArgs)
        {
            _logService.Trace("PlaybackService.OnTrackStarted : " + trackStartedEventArgs.Track.Title);
            _lastStartedTrack = trackStartedEventArgs.Track;
            if (TrackStarted != null)
            {
                TrackStarted(this, trackStartedEventArgs);
            }
        }


        private void BufferizedCallback(Stream trackStream)
        {
            _audioRenderingService.Play(trackStream);
            _status = PlaybackStatus.Playing;
        }

        private void BufferingProgressChanged(double progress)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackService"/> class.
        /// </summary>
        /// <param name="audioRenderingService">The audio rendering service.</param>
        public PlaybackService(AudioRenderingService audioRenderingService)
        {
            _logService = IoC.Container.Get<ILogService>();

            _status = PlaybackStatus.Stopped;


            if (audioRenderingService == null)
            {
                throw new ArgumentNullException("audioRenderingService");
            }

            _audioRenderingService = audioRenderingService;
            _audioRenderingService.BufferingProgressUpdated += (o, e) => OnBufferingProgressUpdated(e);
            _audioRenderingService.MediaPositionChanged += MediaPositionChanged;
            _audioRenderingService.MediaEnded += MediaEnded;
            _audioRenderingService.PlaybackStarted += (sender, eventArgs) => OnTrackStarted(new TrackStartedEventArgs { Track = eventArgs.Track });


            PlayqueueItems = new List<SynoTrack>();
        }

        public event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;


        public SynoTrack GetNextTrack(SynoTrack currentTrack)
        {
            int currentTrackIndex = PlayqueueItems.IndexOf(currentTrack);

            if (PlayqueueItems.Count > currentTrackIndex + 1)
            {
                return PlayqueueItems[currentTrackIndex + 1];
            } 

            // if there is no track next, then we return null.
            return null;
        }

        private void OnBufferingProgressUpdated(BufferingProgressUpdatedEventArgs bufferingProgressUpdatedEventArgs)
        {
            if (BufferingProgressUpdated != null)
            {
                BufferingProgressUpdated(this, bufferingProgressUpdatedEventArgs);
            }
        }

        private void MediaPositionChanged(object sender, MediaPositionChangedEventArgs e)
        {
            OnTrackCurrentPositionChanged(e);
        }

        private void OnTrackCurrentPositionChanged(MediaPositionChangedEventArgs mediaPositionChangedEventArgs)
        {
            if (TrackCurrentPositionChanged != null)
            {
                // Hack : we shouldn't have to resort to use a SynoTrack, but since the MediaStreamSource is buggy, we don't have much of a choice...
                TrackCurrentPositionChanged(this, new TrackCurrentPositionChangedEventArgs { LoadPercentComplete = 1, PlaybackPercentComplete = mediaPositionChangedEventArgs.Position.TotalSeconds / _lastStartedTrack.Duration.TotalSeconds, Position = mediaPositionChangedEventArgs.Position });
                //TrackCurrentPositionChanged(this, new TrackCurrentPositionChangedEventArgs { LoadPercentComplete = 1, PlaybackPercentComplete = mediaPositionChangedEventArgs.Position.TotalSeconds / mediaPositionChangedEventArgs.Duration.TotalSeconds, Position = mediaPositionChangedEventArgs.Position });
            }
        }


        private void MediaEnded(object sender, MediaEndedEventArgs e)
        {
            _status = PlaybackStatus.Stopped;
            var currentTrackItem = PlayqueueItems.IndexOf(e.Track);
            
            if (PlayqueueItems.Count > currentTrackItem + 1)
            {
                var nextTrack = GetNextTrack(e.Track);
                PlayTrackInQueue(nextTrack);
            }
            else
            {
                try
                {
                    PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Enabled;
                }
                catch (InvalidOperationException)
                {
                    // we know that with version 7.0 : this operation is not supported, so instead of testing the version to see if it is supported,
                    // we try it anyway, and if the OS supports it, then good for us. otherwise, it will be just as if we hadn't even tried it.                    
                }
                
            }
            
        }

        public event TrackEndedDelegate TrackEnded;
        public event TrackStartedDelegate TrackStarted;
        public event TrackCurrentPositionChangedDelegate TrackCurrentPositionChanged;
    }
    

    public enum PlaybackStatus
    {
        Playing,
        Stopped, 
        Buffering
    }
}
