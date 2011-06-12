using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Synology.AudioStationApi;

namespace OpenSyno.Services
{
    using System;

    public interface IPlaybackService
    {
        /// <summary>
        /// Gets or sets what strategy should be used to define the next track to play.
        /// </summary>
        /// <value>The playback continuity.</value>
        PlaybackContinuity PlaybackContinuity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the next track to be played should be preloaded.
        /// </summary>
        /// <value><c>true</c> if the next track to be played should be preloaded; otherwise, <c>false</c>.</value>
        bool PreloadTracks { get; set; }

        /// <summary>
        /// Gets the items in the playqueue.
        /// </summary>
        /// <value>The items in the playqueue.</value>
        IList<SynoTrack> PlayqueueItems { get; }

        PlaybackStatus Status { get; }

        /// <summary>
        /// Clears the play queue.
        /// </summary>
        void ClearPlayQueue();

        /// <summary>
        /// Inserts the specified tracks to the play queue.
        /// </summary>
        /// <param name="tracks">The tracks.</param>
        /// <param name="insertPosition"></param>
        void InsertTracksToQueue(IEnumerable<SynoTrack> tracks, int insertPosition);

        /// <summary>
        /// Plays the specified track. It must be present in the queue.
        /// </summary>
        /// <param name="trackToPlay">The track to play.</param>
        void PlayTrackInQueue(SynoTrack trackToPlay);

        event TrackEndedDelegate TrackEnded;

        event TrackStartedDelegate TrackStarted;
        /// <summary>
        /// Occurs when the position of the playing track changes.
        /// </summary>
        event TrackCurrentPositionChangedDelegate TrackCurrentPositionChanged;

        event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;

        SynoTrack GetNextTrack(SynoTrack currentTrack);

        void PausePlayback();

        void ResumePlayback();

        double GetVolume();

        void SetVolume(double volume);
    }

    public delegate void TrackStartedDelegate(object sender, TrackStartedEventArgs args);

    public class TrackStartedEventArgs
    {
        public SynoTrack Track { get; set; }
    }

    public delegate void TrackEndedDelegate(object sender, TrackEndedDelegateArgs args);

    public class TrackEndedDelegateArgs
    {
        public SynoTrack Track { get; set; }
    }
}