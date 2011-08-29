using System.Windows.Data;
using Ninject;
using OpenSyno.Helpers;

namespace OpenSyno.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;

    using Microsoft.Phone.BackgroundAudio;

    using Synology.AudioStationApi;

    public class BackgroundAudioRenderingService : IAudioRenderingService, IDisposable
    {
        private const string PositionPropertyName = "Position";
        private IAudioStationSession _audioStationSession;
        //private MediaElement _mediaElement;

        /// <summary>
        /// A reference to the <see cref="SynoTrack"/> object representing the track being rendered.
        /// </summary>
        /// <remarks>
        /// Because we are using a private member, there can be only one track being played at a time, and therefore, this service is not thread-safe.
        /// </remarks>
        private SynoTrack _currentTrack;

        public event EventHandler<MediaPositionChangedEventArgs> MediaPositionChanged;

        public event EventHandler<MediaEndedEventArgs> MediaEnded;

        public BackgroundAudioRenderingService(IAudioStationSession audioStationSession)
        {
            // todo : inject ?
            _logService = IoC.Container.Get<ILogService>();

            _logService.Trace("BackgroundAudioRenderingService .ctor");
            if (audioStationSession == null)
            {
                throw new ArgumentNullException("audioStationSession");
            }
            _audioStationSession = audioStationSession;

            //_mediaElement = (MediaElement)Application.Current.Resources["MediaElement"];

            BufferPlayableHeuristicPredicate = (track, bytesLoaded) => bytesLoaded >= track.Bitrate || bytesLoaded == track.Size;

            //_mediaElement.MediaFailed += MediaFailed;

            // TODO : Add error handling

            // TODO : Add position handling BackgroundAudioPlayer.Instance.Position
            // _mediaElement.SetBinding(MediaElement.PositionProperty, new Binding { Source = this, Mode = BindingMode.TwoWay, Path = new PropertyPath(PositionPropertyName)  });

            Timer t = new Timer(
                e =>
                {
                    if (this.MediaPositionChanged != null)
                    {
                        this.MediaPositionChanged(
                            this,
                            new MediaPositionChangedEventArgs
                                {
                                    Duration = this._currentTrack.Duration,
                                    Position = this.Position
                                });
                    }
                },
                null,
                0,
                1000);

            BackgroundAudioPlayer.Instance.PlayStateChanged += this.OnPlayStateChanged;

            //_mediaElement.MediaOpened += MediaOpened;

            // todo : handle end of track
            //_mediaElement.MediaEnded += PlayingMediaEnded;

        }

        private void OnPlayStateChanged(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    this.PlayingMediaEnded();
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.TrackReady:
                    break;
                case PlayState.TrackEnded:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
                case PlayState.Shutdown:
                    break;
                case PlayState.Error:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _logService.Trace(string.Format("BackgroundAudioRenderingService.MediaFailed : {0} : {1}", e.ErrorException.GetType().FullName, e.ErrorException.Message));
            if (e.ErrorException.Message == "AG_E_NETWORK_ERROR")
            {
                throw new SynoNetworkException("Open Syno could not complete the operation. Please check that your phone is not in flight mode.", e.ErrorException);
            }

            throw e.ErrorException;
        }

        private TimeSpan _position;
        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnMediaPositionChanged(Position);
            }
        }

        private void OnMediaPositionChanged(TimeSpan position)
        {
            //    if (MediaPositionChanged != null)
            //    {
            //        // FIXME : NaturalDuration has nothing to do with the total duration of the track : Same for the position : it doesn't reflect the position of the current track, ( is it the position in the total duration of the played items ? ) at least in the emulator !
            //        MediaPositionChanged(this, new MediaPositionChangedEventArgs {Position = position, Duration = _mediaElement.NaturalDuration.TimeSpan});
            //    }
        }

        public Func<SynoTrack, long, bool> BufferPlayableHeuristicPredicate { get; set; }

        /// <summary>
        /// The heuristic used to define whether a given buffer can be played.
        /// </summary>
        /// <param name="track">The track being loaded.</param>
        /// <param name="loadedBytes">The amount of loaded bytes.</param>
        /// <returns></returns>
        /// <remarks>The method can be overrided, but the default predicate can also easily be replaced with the <see cref="BufferPlayableHeuristicPredicate"/> property.</remarks>
        public virtual bool BufferPlayableHeuristic(SynoTrack track, long loadedBytes)
        {
            return BufferPlayableHeuristicPredicate(track, loadedBytes);
        }

        public event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;
        bool _isPlayable;
        private readonly ILogService _logService;
        private int _downloadTrackCallbackCount;

        public event EventHandler<PlayBackStartedEventArgs> PlaybackStarted;

        private void PlayingMediaEnded()
        {
            _logService.Trace("BackgroundAudioRenderingService.PlayingMediaEnded : " + _currentTrack.Title);

            if (MediaEnded != null)
            {
                MediaEnded(this, new MediaEndedEventArgs { Track = _currentTrack });
            }
        }



        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            _logService.Trace("BackgroundAudioRenderingService.MediaOpened : " + _currentTrack.Title);
            // TODO : Start timer to update position every second.
        }

        internal class DownloadFileState
        {
            private readonly long _fileSize;

            public Stream SourceStream { get; set; }
            public long BytesLeft { get; set; }
            public Stream TargetStream { get; set; }

            public SynoTrack SynoTrack { get; set; }

            public byte[] Buffer { get; set; }
            public string FilePath { get; set; }
            public long FileSize { get; set; }
            public Action<double> BufferingProgressUpdate { get; set; }

            public DownloadFileState(Stream sourceStream, long bytesLeft, Stream targetStream, SynoTrack synoTrack, byte[] buffer, string filePath, long fileSize, Action<double> bufferingProgressUpdate)
            {
                if (sourceStream == null)
                {
                    throw new ArgumentNullException("sourceStream");
                }

                if (targetStream == null)
                {
                    throw new ArgumentNullException("targetStream");
                }

                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (filePath == null) throw new ArgumentNullException("filePath");
                if (bufferingProgressUpdate == null)
                {
                    throw new ArgumentNullException("bufferingProgressUpdate");
                }
                _fileSize = fileSize;
                SourceStream = sourceStream;
                BytesLeft = bytesLeft;
                TargetStream = targetStream;
                SynoTrack = synoTrack;
                Buffer = buffer;
                FilePath = filePath;
                FileSize = fileSize;
                BufferingProgressUpdate = bufferingProgressUpdate;
            }
        }

        protected void OnPlaybackStarted(PlayBackStartedEventArgs eventArgs)
        {
            if (PlaybackStarted != null)
            {
                PlaybackStarted(this, eventArgs);
            }
        }

        public void Dispose()
        {
            //_mediaElement.CurrentStateChanged -= OnCurrentStateChanged;
            //_mediaElement.MediaOpened -= MediaOpened;
            //_mediaElement.MediaEnded -= PlayingMediaEnded;
        }

        public void Pause()
        {
            BackgroundAudioPlayer.Instance.Pause();
            //_mediaElement.Pause();
        }

        public void Resume()
        {
            BackgroundAudioPlayer.Instance.Play();
        }

        public double GetVolume()
        {
            return BackgroundAudioPlayer.Instance.Volume;
        }

        public void SetVolume(double volume)
        {
            BackgroundAudioPlayer.Instance.Volume = volume;
        }

        public void StreamTrack(SynoTrack trackToPlay)
        {
            // hack : Synology's webserver doesn't accept the + character as a space : it needs a %20, and it needs to have special characters such as '&' to be encoded with %20 as well, so an HtmlEncode is not an option, since even if a space would be encoded properly, an ampersand (&) would be translated into &amp;
            string url = string.Format("http://{0}:{1}/audio/webUI/audio_stream.cgi/0.mp3?sid={2}&action=streaming&songpath={3}", _audioStationSession.Host, _audioStationSession.Port, _audioStationSession.Token.Split('=')[1], HttpUtility.UrlEncode(trackToPlay.Res).Replace("+", "%20"));

            BackgroundAudioPlayer.Instance.Track = new AudioTrack(new Uri(url), trackToPlay.Title, trackToPlay.Artist, trackToPlay.Album, new Uri(trackToPlay.AlbumArtUrl));
            BackgroundAudioPlayer.Instance.Play();

        }
    }

    public class PlayBackStartedEventArgs : EventArgs
    {
        public SynoTrack Track { get; set; }
    }

    public class BufferingProgressUpdatedEventArgs : EventArgs
    {
        public long BytesLeft { get; set; }

        public long FileSize { get; set; }

        public string FileName { get; set; }

        public Stream BufferingStream { get; set; }

        public SynoTrack SynoTrack { get; set; }
    }

    public class MediaPositionChangedEventArgs : EventArgs
    {
        public TimeSpan Position { get; set; }

        public TimeSpan Duration { get; set; }
    }
}