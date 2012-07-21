using System.Runtime.Serialization;
using FlurryWP7SDK;
using Microsoft.Phone.BackgroundAudio;
using OpemSyno.Contracts;
using OpenSyno.Common;

namespace OpenSyno.BackgroundPlaybackAgent
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Windows;
    using System.Xml.Serialization;

    using Ninject;

    using OpenSyno.Contracts.Domain;
    using OpenSyno.Services;

    using Synology.AudioStationApi;

    public class AudioPlayer : AudioPlayerAgent
    {
        private static volatile bool _classInitialized;

        private IPlaybackService _playbackService;
        private IAudioTrackFactory _audioTrackFactory;
        private List<AsciiUriFix> _asciiUriFixes;
        private List<GuidToTrackMapping> _tracksToGuidMapping = new List<GuidToTrackMapping>();
        private PlayqueueInterProcessCommunicationTransporter _playqueueInformation;

        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        public AudioPlayer()
        {
            Api.StartSession("LVXIENQ85782Q11F3UKR");

            // since we are in a background agent : the types registered in the IoC container are not shared.
            IVersionDependentResourcesProvider versionDependentResourcesProvider = new VersionDependentResourcesProvider();

            _audioTrackFactory = new AudioTrackFactory(versionDependentResourcesProvider);

            // TODO : Handle exceptions 
            using (var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                LoadAsciiUriFixes(userStoreForApplication);
                LoadPlayqueue(userStoreForApplication);
            }

            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += AudioPlayer_UnhandledException;
                });

            }
        }

        private void LoadPlayqueue(IsolatedStorageFile userStoreForApplication)
        {
            using (
                IsolatedStorageFileStream playQueueFile = userStoreForApplication.OpenFile(
                    "playqueue.xml", FileMode.OpenOrCreate))
            {
                // here, we can't work with an ISynoTrack :( tightly bound to the implementation, because of serialization issues...
                var dcs = new DataContractSerializer(
                    typeof (PlayqueueInterProcessCommunicationTransporter), new Type[] {typeof (SynoTrack)});


                _playqueueInformation = (PlayqueueInterProcessCommunicationTransporter) dcs.ReadObject(playQueueFile);

                foreach (GuidToTrackMapping pair in _playqueueInformation.Mappings)
                {
                    _tracksToGuidMapping.Add(pair);
                }
            }
        }

        private void LoadAsciiUriFixes(IsolatedStorageFile userStoreForApplication)
        {
            using (
                IsolatedStorageFileStream asciiUriFixes = userStoreForApplication.OpenFile(
                    "AsciiUriFixes.xml", FileMode.OpenOrCreate))
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof (List<AsciiUriFix>));
                //var xs = new XmlSerializer(typeof(PlayqueueInterProcessCommunicationTransporter));

                try
                {
                    _asciiUriFixes = (List<AsciiUriFix>) dcs.ReadObject(asciiUriFixes);
                }
                catch (Exception e)
                {
                    // could not deserialize XML for playlist : let's build an empty list.
                    _asciiUriFixes = new List<AsciiUriFix>();
                }
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Api.LogError("AudioPlayer_UnhandledException", e.ExceptionObject);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// 
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        /// 
        /// Notable playstate events: 
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackEnded:
                    Func<List<GuidToTrackMapping>, AudioTrack, GuidToTrackMapping> defineNextTrackPredicate = (mappings, currentTrack) =>
                        {
                            var guidToTrackMapping = mappings.SingleOrDefault(o => o.Guid == new Guid(currentTrack.Tag));
                            if (guidToTrackMapping == null)
                            {
                                // play queue has been messed with and the current track could not be found, therefore, we cannot find the next one. for now, we'll stop playback.
                                return null;
                            }

                            var index = mappings.IndexOf(guidToTrackMapping);
                            index++;
                            if (index >= mappings.Count)
                            {
                                // no random, no repeat !
                                return null;
                            }
                            return new GuidToTrackMapping { Guid = mappings[index].Guid, Track = mappings[index].Track };
                        };
                    player.Track = GetNextTrack(track, defineNextTrackPredicate);
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Shutdown:
                    // TODO: Handle the shutdown state here (e.g. save state)
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
            }

            NotifyComplete();
        }


        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        /// 
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.Play:
                    if (player.PlayerState != PlayState.Playing)
                    {
                        player.Play(); // throws 0xC00D2EE2 : is it only when the cable is plugged to a pc ?
                    }
                    break;
                case UserAction.Stop:
                    player.Stop();
                    break;
                case UserAction.Pause:
                    if (player.CanPause)
                    {
                        player.Pause();
                    }
                    break;
                case UserAction.FastForward:
                    if (player.CanSeek)
                    {
                        player.FastForward();
                    }
                    break;
                case UserAction.Rewind:
                    if (player.CanSeek)
                    {
                        player.Rewind();
                    }
                    break;
                case UserAction.Seek:
                    if (player.CanSeek)
                    {
                        player.Position = (TimeSpan)param;
                    }
                    break;
                case UserAction.SkipNext:
                    Func<List<GuidToTrackMapping>, AudioTrack, GuidToTrackMapping> defineNextTrackPredicate = (mappings, currentTrack) =>
                    {
                        var index = mappings.IndexOf(mappings.Single(o => o.Guid == new Guid(currentTrack.Tag)));
                        index++;
                        if (index >= mappings.Count)
                        {
                            // no random, no repeat !
                            return null;
                        }
                        return new GuidToTrackMapping { Guid = mappings[index].Guid, Track = mappings[index].Track };

                    };
                    player.Track = GetNextTrack(track, defineNextTrackPredicate);

                    break;
                case UserAction.SkipPrevious:
                    Func<List<GuidToTrackMapping>, AudioTrack, GuidToTrackMapping> definePreviousTrackPredicate = (mappings, currentTrack) =>
                    {
                        var index = mappings.IndexOf(mappings.Single(o => o.Guid == new Guid(currentTrack.Tag)));
                        index--;
                        if (index < 0)
                        {
                            // no random, no repeat !
                            return null;
                        }
                        return new GuidToTrackMapping { Guid = mappings[index].Guid, Track = mappings[index].Track };

                    };

                    AudioTrack previousTrack = GetPreviousTrack(track, definePreviousTrackPredicate);
                    if (previousTrack != null)
                    {
                        player.Track = previousTrack;
                    }
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Implements the logic to get the next AudioTrack instance.
        /// In a playlist, the source can be from a file, a web request, etc.
        /// </summary>
        /// <param name="audioTrack"></param>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if the playback is completed</returns>
        private AudioTrack GetNextTrack(AudioTrack audioTrack, Func<List<GuidToTrackMapping>, AudioTrack, GuidToTrackMapping> defineNextTrackPredicate)
        {
            if (defineNextTrackPredicate == null)
            {
                throw new ArgumentNullException("defineNextTrackPredicate");
            }

            if (audioTrack != null && !string.IsNullOrWhiteSpace(audioTrack.Tag))
            {

                GuidToTrackMapping guidToTrackMapping = defineNextTrackPredicate(_tracksToGuidMapping, audioTrack);

                if (guidToTrackMapping != null)
                {
                    AudioTrack track;
                    SynoTrack nextTrack = guidToTrackMapping.Track;

                    // is there a fix we can apply ?
                    if (_asciiUriFixes.Any(fix => fix.Res == nextTrack.Res))
                    {
                        AsciiUriFix asciiUriFix = this._asciiUriFixes.Single(fix => fix.Res == nextTrack.Res);
                        if (asciiUriFix.Url == null)
                        {
                            throw new NotSupportedException("We knew this day will come eventually : we just imagined that this would not happen anytime soon, so being agile, we didn't implement it yet : we need to support the scenario where a track is started from the phone's UI instead of the app's. (AudioPlayer.cs::GetNextTrack()");
                        }
                        track = _audioTrackFactory.Create(nextTrack, guidToTrackMapping.Guid, _playqueueInformation.Host, _playqueueInformation.Port, _playqueueInformation.Token, asciiUriFix.Url);
                    }
                    else
                    {
                        track = _audioTrackFactory.Create(nextTrack, guidToTrackMapping.Guid, _playqueueInformation.Host, _playqueueInformation.Port, _playqueueInformation.Token);

                    }
                    // new AudioTrack(new Uri(nextTrack.Res), nextTrack.Title, nextTrack.Artist, nextTrack.Album, new Uri(nextTrack.AlbumArtUrl), guidToTrackMapping.Guid.ToString(), EnabledPlayerControls.All);
                    // new AudioTrack(new Uri(nextTrack.Res), nextTrack.Title, nextTrack.Artist, nextTrack.Album, new Uri(nextTrack.AlbumArtUrl), guidToTrackMapping.Guid.ToString(), EnabledPlayerControls.All);

                    return track;
                }
            }


            // specify the track

            return null;
        }


        /// <summary>
        /// Implements the logic to get the previous AudioTrack instance.
        /// </summary>
        /// <remarks>
        /// The AudioTrack URI determines the source, which can be:
        /// (a) Isolated-storage file (Relative URI, represents path in the isolated storage)
        /// (b) HTTP URL (absolute URI)
        /// (c) MediaStreamSource (null)
        /// </remarks>
        /// <returns>an instance of AudioTrack, or null if previous track is not allowed</returns>
        private AudioTrack GetPreviousTrack(AudioTrack audioTrack, Func<List<GuidToTrackMapping>, AudioTrack, GuidToTrackMapping> definePreviousTrackPredicate)
        {
            if (definePreviousTrackPredicate == null)
            {
                throw new ArgumentNullException("definePreviousTrackPredicate");
            }


            AudioTrack track = null;
            if (audioTrack != null && !string.IsNullOrWhiteSpace(audioTrack.Tag))
            {

                GuidToTrackMapping guidToTrackMapping = definePreviousTrackPredicate(_tracksToGuidMapping, audioTrack);

                if (guidToTrackMapping != null)
                {
                    SynoTrack nextTrack = guidToTrackMapping.Track;

                    // is there a fix we can apply ?
                    if (_asciiUriFixes.Any(fix => fix.Res == nextTrack.Res))
                    {
                        track = _audioTrackFactory.Create(nextTrack, guidToTrackMapping.Guid, _playqueueInformation.Host, _playqueueInformation.Port, _playqueueInformation.Token, _asciiUriFixes.Single(fix => fix.Res == nextTrack.Res).Url);
                    }
                    else
                    {
                        track = _audioTrackFactory.Create(nextTrack, guidToTrackMapping.Guid, _playqueueInformation.Host, _playqueueInformation.Port, _playqueueInformation.Token);

                    }
                    // new AudioTrack(new Uri(nextTrack.Res), nextTrack.Title, nextTrack.Artist, nextTrack.Album, new Uri(nextTrack.AlbumArtUrl), guidToTrackMapping.Guid.ToString(), EnabledPlayerControls.All);
                    // new AudioTrack(new Uri(nextTrack.Res), nextTrack.Title, nextTrack.Artist, nextTrack.Album, new Uri(nextTrack.AlbumArtUrl), guidToTrackMapping.Guid.ToString(), EnabledPlayerControls.All);                    
                }
            }

            return track;
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            if (isFatal)
            {
                Abort();
            }
            else
            {
                NotifyComplete();
            }

        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        /// <remarks>
        /// Once the request is Cancelled, the agent gets 5 seconds to finish its work,
        /// by calling NotifyComplete()/Abort().
        /// </remarks>
        protected override void OnCancel()
        {

        }
    }

    [DataContract]
    public class PlayqueueInterProcessCommunicationTransporter
    {
        public PlayqueueInterProcessCommunicationTransporter()
        {
            Mappings = new List<GuidToTrackMapping>();
        }
        [DataMember]
        public string Host { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public List<GuidToTrackMapping> Mappings { get; set; }
    }
}
