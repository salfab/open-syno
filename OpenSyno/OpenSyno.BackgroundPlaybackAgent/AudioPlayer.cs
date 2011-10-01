﻿using System.Runtime.Serialization;
using Microsoft.Phone.BackgroundAudio;
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

        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        public AudioPlayer()
        {
            _audioTrackFactory = new AudioTrackFactory();
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

        /// Code to execute on Unhandled Exceptions
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
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
                    Func<Dictionary<Guid, SynoTrack>, AudioTrack, GuidToTrackMapping> defineNextTrackPredicate = (dict, currentTrack) =>
                        {
                            var index = dict.Keys.ToList().IndexOf(new Guid(currentTrack.Tag));
                            index++;
                            if (index >= dict.Count)
                            {
                                // no random, no repeat !
                                return null;
                            }
                            return new GuidToTrackMapping() { Guid = dict.ToArray().ElementAt(index).Key, Track = dict.ToArray().ElementAt(index).Value };
                        };
                    GetNextTrack(track, defineNextTrackPredicate, t => { player.Track = t; }, e => { throw e; });
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
                        player.Play();
                    }
                    break;
                case UserAction.Stop:
                    player.Stop();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.FastForward:
                    player.FastForward();
                    break;
                case UserAction.Rewind:
                    player.Rewind();
                    break;
                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
                case UserAction.SkipNext:
                        Func<Dictionary<Guid, SynoTrack>, AudioTrack, GuidToTrackMapping> defineNextTrackPredicate = (dict, currentTrack) =>
                        {
                            var index = dict.Keys.ToList().IndexOf(new Guid(currentTrack.Tag));
                            index++;
                            if (index == dict.Count())
                            {
                                return null;
                            }
                            return new GuidToTrackMapping() { Guid = dict.ToArray().ElementAt(index).Key, Track = dict.ToArray().ElementAt(index).Value };
                        };
                        GetNextTrack(track, defineNextTrackPredicate, t => { player.Track = t; }, e => { throw e; });
                    break;
                case UserAction.SkipPrevious:
                    AudioTrack previousTrack = GetPreviousTrack();
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
        private void GetNextTrack(AudioTrack audioTrack, Func<Dictionary<Guid, SynoTrack>, AudioTrack, GuidToTrackMapping> defineNextTrackPredicate, Action<AudioTrack> successCallback, Action<Exception> errorCallback)
        {
            if (defineNextTrackPredicate == null)
            {
                throw new ArgumentNullException("defineNextTrackPredicate");
            }
            var tracksToGuidMapping = new Dictionary<Guid, SynoTrack>();

            PlayqueueInterProcessCommunicationTransporter deserialization = null;
            using(IsolatedStorageFileStream playQueueFile = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("playqueue.xml", FileMode.OpenOrCreate))
            {
                // here, we can't work with an ISynoTrack :( tightly bound to the implementation, because of serialization issues...
                var dcs = new DataContractSerializer(typeof(PlayqueueInterProcessCommunicationTransporter), new Type[] { typeof(SynoTrack) });

                try
                {
                    deserialization = (PlayqueueInterProcessCommunicationTransporter)dcs.ReadObject(playQueueFile);

                    foreach (GuidToTrackMapping pair in deserialization.Mappings)
                    {
                        tracksToGuidMapping.Add(pair.Guid, pair.Track);
                    }
                }
                catch (Exception e)
                {
                    // no more tracks
                    successCallback(null); 
                }
            }
            SynoTrack mappedTrack;
            if (audioTrack != null && !string.IsNullOrWhiteSpace(audioTrack.Tag))
            {

                GuidToTrackMapping guidToTrackMapping = defineNextTrackPredicate(tracksToGuidMapping, audioTrack);

                if (guidToTrackMapping != null )
                {

                    SynoTrack nextTrack = guidToTrackMapping.Track;

                    _audioTrackFactory.BeginCreate(
                        nextTrack,
                        guidToTrackMapping.Guid, 
                        deserialization.Protocol,
                        deserialization.Host,
                        deserialization.Port,
                        deserialization.Token,
                        successCallback,
                        errorCallback);
                    // new AudioTrack(new Uri(nextTrack.Res), nextTrack.Title, nextTrack.Artist, nextTrack.Album, new Uri(nextTrack.AlbumArtUrl), guidToTrackMapping.Guid.ToString(), EnabledPlayerControls.All);
                }
            }


            // specify the track

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
        private AudioTrack GetPreviousTrack()
        {
            // TODO: add logic to get the previous audio track

            AudioTrack track = null;

            // specify the track

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
        [DataMember]
        public string Protocol { get; set; }
    }
}
