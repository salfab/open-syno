namespace OpemSyno.Contracts
{
    using Synology.AudioStationApi;

    public interface IAlbumViewModelFactory
    {
        IAlbumViewModel Create(SynoItem album);
    }
}