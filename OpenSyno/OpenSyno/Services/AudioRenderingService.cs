using System.Diagnostics;
using System.Windows.Data;
using Media;
using Ninject;
using OpenSyno.Helpers;

namespace OpenSyno.Services
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Net;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using Synology.AudioStationApi;

    public class AudioRenderingService : IAudioRenderingService, IDisposable
    {
        private const string PositionPropertyName = "Position";
        private IAudioStationSession _audioStationSession;
        private MediaElement _mediaElement;

        /// <summary>
        /// A reference to the <see cref="SynoTrack"/> object representing the track being rendered.
        /// </summary>
        /// <remarks>
        /// Because we are using a private member, there can be only one track being played at a time, and therefore, this service is not thread-safe.
        /// </remarks>
        private SynoTrack _currentTrack;

        public event EventHandler<MediaPositionChangedEventArgs> MediaPositionChanged;

        public event EventHandler<MediaEndedEventArgs> MediaEnded;

        public AudioRenderingService(IAudioStationSession audioStationSession)
        {
            // todo : inject ?
            _logService = IoC.Container.Get<ILogService>();

            _logService.Trace("AudioRenderingService .ctor");
            if (audioStationSession == null)
            {
                throw new ArgumentNullException("audioStationSession");
            }
            _audioStationSession = audioStationSession;

            _mediaElement = (MediaElement)Application.Current.Resources["MediaElement"];

            // HACK : 500kb is absolutely arbitrary and is supposed to cover for the mp3 header and the ID3 tags. a more subtle approach would be to retrieve the actual size of the header for our heuristic to be more accurate.
            BufferPlayableHeuristicPredicate = (track, bytesLoaded) =>  bytesLoaded >= 1||  bytesLoaded == track.Size;

            _mediaElement.MediaFailed += MediaFailed;

            _mediaElement.SetBinding(MediaElement.PositionProperty, new Binding { Source = this, Mode = BindingMode.TwoWay, Path = new PropertyPath(PositionPropertyName)  });
            _mediaElement.CurrentStateChanged += OnCurrentStateChanged;
            _mediaElement.MediaOpened += MediaOpened;
            _mediaElement.MediaEnded += PlayingMediaEnded;
        }

        private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _logService.Trace(string.Format("AudioRenderingService.MediaFailed : {0} : {1}", e.ErrorException.GetType().FullName, e.ErrorException.Message));
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
            if (MediaPositionChanged != null)
            {
                // FIXME : NaturalDuration has nothing to do with the total duration of the track : Same for the position : it doesn't reflect the position of the current track, ( is it the position in the total duration of the played items ? ) at least in the emulator !
                MediaPositionChanged(this, new MediaPositionChangedEventArgs {Position = position, Duration = _mediaElement.NaturalDuration.TimeSpan});
            }
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

        /// <summary>
        /// Downloads to temporary file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="contentLength">Length of the content.</param>
        /// <param name="targetStream">The target stream.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="bufferingProgressUpdate">The buffering progress update.</param>
        private void DownloadToTemporaryFile(Stream stream, long contentLength, Stream targetStream, SynoTrack synoTrack, string filePath, Action<double> bufferingProgressUpdate)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (targetStream == null)
            {
                throw new ArgumentNullException("targetStream");
            }

            // 4096 works better on the device
            int bufferSize = 4096;
            var buffer = new byte[bufferSize];


            long bytesLeft = contentLength;
            while (bytesLeft > 0)
            {
                var readCount = stream.Read(buffer, 0, buffer.Length);
                if (readCount < bufferSize && readCount < bytesLeft)
                {
                    _logService.Trace(string.Format("Network stream starving : {0} bytes read; Throttling download.",readCount));
                    // throttle the download
                    Thread.Sleep(3000);
                }
                _logService.ConditionalTrace("RWMS_STARVING", "AudioRenderingService.DownloadTrackCallback : readCount = " + readCount);

                bytesLeft = bytesLeft - readCount;

                // Note : this targetstream is protected against synchronous concurrent reading/writing, since it is a ReadWriteMemoryStream.
                targetStream.Write(buffer, 0, readCount);

                var bufferingProgressUpdatedEventArgs = new BufferingProgressUpdatedEventArgs
                {
                    BytesLeft = bytesLeft,
                    FileSize = contentLength,
                    FileName = filePath,
                    BufferingStream = targetStream,
                    SynoTrack = synoTrack
                };
                _downloadTrackCallbackCount++;

                Deployment.Current.Dispatcher.BeginInvoke(() => OnBufferingProgressUpdated(bufferingProgressUpdatedEventArgs));
            }
            stream.Close();
            stream.Dispose();
        }

        public event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;
        bool _isPlayable;
        private readonly ILogService _logService;
        private int _downloadTrackCallbackCount;

        public event EventHandler<PlayBackStartedEventArgs> PlaybackStarted;

        private void OnBufferingProgressUpdated(BufferingProgressUpdatedEventArgs bufferingProgressUpdatedEventArgs)
        {
            if (BufferingProgressUpdated != null)
            {
                BufferingProgressUpdated(this, bufferingProgressUpdatedEventArgs);
            }

            if (_isPlayable || bufferingProgressUpdatedEventArgs.SynoTrack != _currentTrack)
            {
                return;
            }

            // FIXME : use event args, not instance fields, and maybe pass stream along or something similar to make sure we're comparing the right stream buffering progress
            _isPlayable = this.BufferPlayableHeuristic(bufferingProgressUpdatedEventArgs.SynoTrack, bufferingProgressUpdatedEventArgs.FileSize - bufferingProgressUpdatedEventArgs.BytesLeft);
            if (_isPlayable)
            {
                _logService.Trace("AudioRenderingService.OnBufferingProgressUpdated : Media is playable");
                OnBufferReachedPlayableState(bufferingProgressUpdatedEventArgs.BufferingStream, bufferingProgressUpdatedEventArgs.SynoTrack);
            }


        }

        private void OnCurrentStateChanged(object sender, RoutedEventArgs e)
        {
            var state = ((MediaElement)sender).CurrentState;
            _logService.Trace("AudioRenderingService.OnCurrentStateChanged : " + state);
        }


        private void PlayingMediaEnded(object sender, RoutedEventArgs e)
        {
            _logService.Trace("AudioRenderingService.PlayingMediaEnded : " + _currentTrack.Title);

            if (MediaEnded != null)
            {
                MediaEnded(this, new MediaEndedEventArgs { Track = _currentTrack });
            }
        }



        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            _logService.Trace("AudioRenderingService.MediaOpened : " + _currentTrack.Title);
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

            public DownloadFileState(Stream sourceStream, long bytesLeft, Stream targetStream,SynoTrack synoTrack, byte[] buffer, string filePath, long fileSize, Action<double> bufferingProgressUpdate)
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

        public void Bufferize(Action<Stream> bufferizedCallback, Action<double> bufferizeProgressChangedCallback , SynoTrack track)
        {
            _logService.Trace("AudioRenderingService.Bufferize : " + track.Title);

            if (bufferizedCallback == null)
            {
                throw new ArgumentNullException("bufferizedCallback");
            }
            if (bufferizeProgressChangedCallback == null)
            {
                throw new ArgumentNullException("bufferizeProgressChangedCallback");
            }
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            _currentTrack = track;

            _audioStationSession.GetFileStream(track, OnFileStreamOpened);

            // TODO : Start download
        }

        private void OnFileStreamOpened(WebResponse response, SynoTrack synoTrack)
        {
            Thread.CurrentThread.Name = "Downloader";
            _logService.Trace("AudioRenderingService.OnFileStreamOpened : " + synoTrack.Title);

            var trackStream = response.GetResponseStream();
            
            // The trackstream is not readable.
            _isPlayable = false;

            _logService.Trace("AudioRenderingService.OnFileStreamOpened : Instanciating ReadWriteMemoryStream of size " + response.ContentLength);
            Stream targetStream = new ReadWriteMemoryStream((int)response.ContentLength);

            DownloadToTemporaryFile(trackStream, response.ContentLength, targetStream, synoTrack, string.Empty, BufferedProgressUpdated);
            
        }

        private void BufferedProgressUpdated(double obj)
        {
           
        }

        private void OnBufferReachedPlayableState(Stream stream, SynoTrack synoTrack)
        {
            _logService.Trace("AudioRenderingService.OnBufferReachedPlayableState : " + synoTrack.Title);
            // Hack : for now we just avoid it to crash : it seems that not continuing the download is not enough since an pother thread might still be running
            // and continuing once too much and make everything crash : 
            // here, by fixing this that way, we'll have some side effects : the download will continue and the download progress bar will show alternatively both statuses... 
            // we definitely must fix this an other way :)
            if (stream.CanRead)
            {
                Delegate mediaRenderingStarter = new Action<Stream>(streamToPlay =>
                                                                          {
                                                                              _logService.Trace("AudioRenderingService.OnBufferReachedPlayableState : starting mediaRenderingStarter delegate");
                                                                              _mediaElement.Stop();
                                                                              Mp3MediaStreamSource mss = new Mp3MediaStreamSource(streamToPlay);

                                                                              _logService.Trace("AudioRenderingService.OnBufferReachedPlayableState : setting mediaelement's source");
                                                                              _mediaElement.SetSource(mss);
                                                                              _logService.Trace("AudioRenderingService.OnBufferReachedPlayableState : mediaelement's source set");

                                                                              _mediaElement.Position = TimeSpan.FromSeconds(0);
                                                                              _mediaElement.Volume = 20;

                                                                              _logService.Trace("AudioRenderingService.OnBufferReachedPlayableState : starting mediaelement playback.");
                                                                              _mediaElement.Play();
                                                                              _logService.Trace("AudioRenderingService.OnBufferReachedPlayableState : mediaelement playback started.");
                                                                              PlayBackStartedEventArgs eventArgs = new PlayBackStartedEventArgs();
                                                                              eventArgs.Track = synoTrack;
                                                                              OnPlaybackStarted(eventArgs);
                                                                          });
                Deployment.Current.Dispatcher.BeginInvoke(mediaRenderingStarter, new object[] {stream});
            }
        }

        protected void OnPlaybackStarted(PlayBackStartedEventArgs eventArgs)
        {
            if (PlaybackStarted != null)
            {
                PlaybackStarted(this, eventArgs);
            }
        }

        public void Play(Stream trackStream)
        {
            // TODO : ask the media element to start playing.
            _mediaElement.Stop();
            MediaStreamSource mms = new Mp3MediaStreamSource(trackStream);
            _mediaElement.SetSource(mms);

            _mediaElement.Position = TimeSpan.FromSeconds(0);
            _mediaElement.Volume = 20;
            _mediaElement.Play();
        }

        public void Dispose()
        {            
            _mediaElement.CurrentStateChanged -= OnCurrentStateChanged;
            _mediaElement.MediaOpened -= MediaOpened;
            _mediaElement.MediaEnded -= PlayingMediaEnded;
        }

        public void Pause()
        {
            _mediaElement.Pause();
        }

        public void Resume()
        {
            _mediaElement.Play();
        }

        public double GetVolume()
        {
            return _mediaElement.Volume;
        }

        public void SetVolume(double volume)
        {
            _mediaElement.Volume = volume;
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