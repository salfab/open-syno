namespace OpenSyno.Services
{
    using System;

    public class MediaEndedEventArgs : EventArgs
    {
        public ISynoTrack Track { get; set; }
    }
}