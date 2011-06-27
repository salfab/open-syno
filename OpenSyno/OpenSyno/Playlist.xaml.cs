namespace OpenSyno
{
    using System.Collections.Generic;

    using OpenSyno.ViewModels;

    public class Playlist
    {
        public IEnumerable<TrackViewModel> Tracks { get; set; }
    }
}