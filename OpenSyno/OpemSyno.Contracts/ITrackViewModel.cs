namespace OpemSyno.Contracts
{
    using System;
    using System.Runtime.Serialization;
    using System.Windows.Input;

    using Synology.AudioStationApi;

    public interface ITrackViewModel
    {
        Guid Guid { get; set; }

        [DataMember]
        SynoTrack TrackInfo { get; set; }

        [DataMember]
        bool IsSelected { get; set; }

        ICommand NavigateToContainingAlbumCommand { get; set; }

        /// <summary>
        /// Gets a guid which is the same across consecutive tracks that are part of the same album.
        /// </summary>
        /// <remarks>This property can be used to group tracks by album *ONLY* if they are adjacent in the playqueue.</remarks>
        /// <value>The consecutive album identifier.</value>
        [DataMember]
        Guid ConsecutiveAlbumIdentifier { get; set; }
    }
}