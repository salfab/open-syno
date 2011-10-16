namespace OpemSyno.Contracts
{
    using System.Collections.Generic;

    using Synology.AudioStationApi;

    public interface IArtistPanoramaAlbumDetailItemFactory
    {
        IArtistPanoramaAlbumDetailItem Create(SynoItem album, IEnumerable<IAlbumViewModel> albums, int activeItemIndex);
    }
}