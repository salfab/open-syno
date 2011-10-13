namespace OpemSyno.Contracts
{
    using System.Collections.ObjectModel;

    using OpemSyno.Contracts.Domain;

    using Synology.AudioStationApi;

    public interface IArtistDetailViewModel
    {
        string ArtistName { get; set; }

        ObservableCollection<IAlbumViewModel> Albums { get; set; }

        ObservableCollection<IArtistViewModel> SimilarArtists { get; set; }
    }
}