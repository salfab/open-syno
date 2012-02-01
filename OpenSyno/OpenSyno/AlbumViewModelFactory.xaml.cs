namespace OpenSyno
{
    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public class AlbumViewModelFactory
    {
        public AlbumViewModel Create(SynoItem album)
        {
            return new AlbumViewModel(album);
        }
    }
}