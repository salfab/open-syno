namespace OpenSyno.Contracts.Domain
{
    using System;
    using System.Runtime.Serialization;

    using OpenSyno.Services;

    [DataContract]
    public class GuidToTrackMapping
    {
        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public ISynoTrack Track { get; set; }
    }
}