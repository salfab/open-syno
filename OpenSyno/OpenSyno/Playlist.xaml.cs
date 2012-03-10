namespace OpenSyno
{
    using System;
    using System.Collections.Generic;

    using OpenSyno.ViewModels;

    public class Playlist
    {
        public List<TrackViewModel> Tracks { get; set; }

        public string Name { get; set; }

        public Guid Id { get; set; }

        public Playlist() : this(Guid.NewGuid(), string.Empty)
        {
        }

        public Playlist(Guid playlistIt, string unsavedPlayqueue)
        {
            this.Name = unsavedPlayqueue;
            Tracks = new List<TrackViewModel>();
        }
    }
}