namespace OpenSyno.Contracts.Domain
{
    using System;
    using System.Runtime.Serialization;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    [DataContract]
    public class GuidToTrackMapping
    {
        public GuidToTrackMapping(Guid guid, SynoTrack track) : this()
        {
            Guid = guid;
            Track = track;
        }

        public GuidToTrackMapping()
        {
        }

        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public SynoTrack Track { get; set; }
    }
}