namespace OpenSyno
{
    using OpemSyno.Contracts;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public class AlbumViewModelFactory : IAlbumViewModelFactory
    {
        public IAlbumViewModel Create(SynoItem album)
        {
            return new AlbumViewModel(album);
        }
    }
}