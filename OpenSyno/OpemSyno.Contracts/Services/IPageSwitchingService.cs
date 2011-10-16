namespace OpenSyno.Services
{
    public interface IPageSwitchingService
    {
        void NavigateToSearchResults();
        void NavigateToArtistPanorama(string artistId, string albumId, string albumsListTicket);
        void NavigateToPreviousPage();

        void NavigateToAboutBox();
        void NavigateToSearchAllResults(string keyword);
        void NavigateToSearch();
        void NavigateToPlayQueue();

        void NavigateToArtistDetailView(string artistId);
    }
}