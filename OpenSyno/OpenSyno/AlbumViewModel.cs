using Synology.AudioStationApi;

namespace OpenSyno
{
    using OpenSyno.Services;

    public class AlbumViewModel
    {
        public AlbumViewModel(SynoItem album)
        {
            this.Album = album;
        }

        public SynoItem Album { get; set; }
    }
}