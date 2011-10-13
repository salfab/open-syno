using System;
using Microsoft.Practices.Prism.Events;

namespace OpenSyno.Services
{
    public class PageSwitchingService : IPageSwitchingService
    {
        private readonly IEventAggregator _eventAggregator;

        private const string ArtistDetailUri = "/ArtistDetailView.xaml?artistTicket={0}";

        private const string SearchUri = "/SearchView.xaml";

        private const string SearchAllResultsUri = "/SearchAllResultsView.xaml?keyword={0}";

        private const string AboutBoxUri = "/AboutBoxView.xaml";

        private const string PlayQueueResultsUri = "/PlayQueueView.xaml";

        private const string ArtistPanoramaUri = "/ArtistPanoramaView.xaml?artistTicket={0}&albumTicket={1}";
        private const string SearchResultsUri = "/SearchResultsView.xaml";

        public PageSwitchingService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void NavigateToSearchResults()
        {
             NavigateToUri(new Uri(SearchResultsUri, UriKind.RelativeOrAbsolute));                        
        }

        public void NavigateToArtistPanorama(string artistId, string albumId)
        {
             NavigateToUri(new Uri(string.Format(ArtistPanoramaUri, artistId, albumId), UriKind.RelativeOrAbsolute));
        }

        public void NavigateToPreviousPage()
        {
            _eventAggregator.GetEvent<CompositePresentationEvent<PageSwitchedAggregatedEvent>>().Publish(new PageSwitchedAggregatedEvent { UseNavigationServiceOperation = true, NavigationServiceOperation =  PageSwitchedAggregatedEvent.NavigationServiceOperations.GoBack});                               

        }

        public void NavigateToAboutBox()
        {
            NavigateToUri(new Uri(AboutBoxUri, UriKind.RelativeOrAbsolute));
        }

        public void NavigateToSearchAllResults(string keyword)
        {
             NavigateToUri(new Uri(string.Format(SearchAllResultsUri, keyword), UriKind.RelativeOrAbsolute));
        }

        public void NavigateToSearch()
        {
            NavigateToUri(new Uri(SearchUri, UriKind.RelativeOrAbsolute));
        }

        public void NavigateToPlayQueue()
        {
            NavigateToUri(new Uri(PlayQueueResultsUri, UriKind.RelativeOrAbsolute));                        
        }

        public void NavigateToArtistDetailView(string artistId)
        {
            NavigateToUri(new Uri(string.Format(ArtistDetailUri, artistId), UriKind.RelativeOrAbsolute));
        }

        private void NavigateToUri(Uri uri)
        {
            _eventAggregator.GetEvent<CompositePresentationEvent<PageSwitchedAggregatedEvent>>().Publish(new PageSwitchedAggregatedEvent { Uri = uri, UseNavigationServiceOperation = false});                               
        }
    }
}