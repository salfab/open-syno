namespace OpenSyno.Services
{
    using System;

    using Synology.AudioStationApi;

    public class MediaEndedEventArgs : EventArgs
    {
        public SynoTrack Track { get; set; }
    }
}