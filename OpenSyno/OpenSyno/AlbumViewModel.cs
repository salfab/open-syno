using Synology.AudioStationApi;

namespace OpenSyno
{
    using OpenSyno.Services;

    public class AlbumViewModel
    {
        public AlbumViewModel(ISynoItem album)
        {
            this.Album = album;
        }

        public ISynoItem Album { get; set; }
    }
}