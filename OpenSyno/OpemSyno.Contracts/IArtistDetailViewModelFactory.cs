namespace OpemSyno.Contracts
{
    using Synology.AudioStationApi;

    public interface IArtistDetailViewModelFactory
    {
        IArtistDetailViewModel Create(SynoItem artist);
    }
}