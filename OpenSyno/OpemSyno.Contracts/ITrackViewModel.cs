namespace OpemSyno.Contracts
{
    using System;
    using System.Runtime.Serialization;

    using Synology.AudioStationApi;

    public interface ITrackViewModel
    {
        Guid Guid { get; set; }

        [DataMember]
        SynoTrack TrackInfo { get; set; }

        [DataMember]
        bool IsSelected { get; set; }
    }
}