namespace OpenSyno.Contracts.Domain
{
    using System;
    using System.Runtime.Serialization;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    /// <summary>
    /// Encapsulates a synotrack and an unique identifier which will tell apart two different instances of the same syno track in a playqueue (2 same songs in a queue)
    /// </summary>
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