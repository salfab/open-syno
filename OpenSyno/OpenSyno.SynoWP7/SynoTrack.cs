using System;

namespace Synology.AudioStationApi
{
    using System.Runtime.Serialization;

    // For serialization purposes
    [KnownType(typeof(SynoItem))]
    [DataContract]
    public class SynoTrack : SynoItem
    {
        [DataMember]
        public string Album { get; set; }

        [DataMember]
        public string Artist { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; }

        [DataMember]
        public string Genre { get; set; }

        [DataMember]
        public int Disc { get; set; }

        [DataMember]
        public string Class { get; set; }

        [DataMember]
        public int Channels { get; set; }

        [DataMember]
        public long Bitrate { get; set; }

        [DataMember]
        public string ProtocolInfo { get; set; }

        [DataMember]
        public string Res { get; set; }

        [DataMember]
        public long Sample { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        public int Track { get; set; }

        [DataMember]
        public int Year { get; set; }
    }
}
