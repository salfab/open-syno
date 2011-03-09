using Synology.AudioStationApi;

namespace OpenSyno
{
    public class AlbumViewModel
    {
        public AlbumViewModel(SynoItem album)
        {
            this.Album = album; 
        }
          
        public SynoItem Album { get; set; }
    }
}