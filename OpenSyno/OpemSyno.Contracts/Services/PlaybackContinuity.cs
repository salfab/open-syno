namespace OpenSyno.Services
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    using Synology.AudioStationApi;

    public enum PlaybackContinuity
    {
        Normal, 
        Random,
        Repeat, 
        RepeatOne
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
    
    //public interface ISynoTrack
    //{
    //    string Album { get; set; }

    //    string Artist { get; set; }

    //    TimeSpan Duration { get; set; }

    //    string Genre { get; set; }

    //    int Disc { get; set; }

    //    string Class { get; set; }

    //    int Channels { get; set; }

    //    long Bitrate { get; set; }

    //    string ProtocolInfo { get; set; }

    //    string Res { get; set; }

    //    long Sample { get; set; }

    //    long Size { get; set; }

    //    int Track { get; set; }

    //    int Year { get; set; }

    //    /// <summary>
    //    /// Gets or sets the title.
    //    /// </summary>
    //    /// <value>The title.</value>
    //    string Title { get; set; }

    //    /// <summary>
    //    /// Gets or sets the album art URL.
    //    /// </summary>
    //    /// <value>The album art URL.</value>
    //    string AlbumArtUrl { get; set; }

    //    /// <summary>
    //    /// Gets or sets the icon.
    //    /// </summary>
    //    /// <value>The item icon.</value>
    //    string Icon { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value indicating whether this instance is container.
    //    /// </summary>
    //    /// <value>
    //    /// <c>true</c> if this instance is container; otherwise, <c>false</c>.
    //    /// </value>
    //    bool IsContainer { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value indicating whether this instance is track.
    //    /// </summary>
    //    /// <value><c>true</c> if this instance is track; otherwise, <c>false</c>.</value>
    //    bool IsTrack { get; set; }

    //    /// <summary>
    //    /// Gets or sets the item ID.
    //    /// </summary>
    //    /// <value>The item ID.</value>
    //    string ItemID { get; set; }

    //    /// <summary>
    //    /// Gets or sets the item pid.
    //    /// </summary>
    //    /// <value>The item pid.</value>
    //    string ItemPid { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value indicating whether this <see cref="SynoItem"/> is support.
    //    /// </summary>
    //    /// <value><c>true</c> if support; otherwise, <c>false</c>.</value>
    //    bool Support { get; set; }

    //    /// <summary>
    //    /// Gets or sets the sequence.
    //    /// </summary>
    //    /// <value>The sequence.</value>
    //    int Sequence { get; set; }
    //}
}