using System;

namespace Synology.AudioStationApi
{
    public class SynoTrack : SynoItem
    {
        public string Album { get; set; }

        public string Artist { get; set; }

        public TimeSpan Duration { get; set; }

        public string Genre { get; set; }

        public int Disc { get; set; }

        public string Class { get; set; }

        public int Channels { get; set; }

        public long Bitrate { get; set; }

        public string ProtocolInfo { get; set; }

        public string Res { get; set; }

        public long Sample { get; set; }

        public long Size { get; set; }

        public int Track { get; set; }

        public int Year { get; set; }
    }
}
