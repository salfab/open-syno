using System;
using System.Windows;
using Ninject;
using OpenSyno.Services;

namespace OpenSyno
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.Phone.Controls;

    using OpemSyno.Contracts;
    using OpemSyno.Contracts.Domain;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public partial class ArtistPanoramaView : PhoneApplicationPage
    {
        private bool _newPageInstance = false;
        private const string ArtistPanoramaViewCurrentArtist = "ArtistPanoramaViewCurrentArtist";

        private SynoItem _artist;

        private IEnumerable<SynoItem> _artistItems;

        private IArtistPanoramaAlbumDetailItemFactory artistPanoramaAlbumDetailItemFactory;

        private const string ArtistPanoramaViewActivePanelIndex = "ArtistPanoramaViewActivePanelIndex";

        private const string ArtistPanoramaViewItems = "ArtistPanoramaViewItems";

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistPanoramaView"/> class.
        /// </summary>
        public ArtistPanoramaView()
        {
            artistPanoramaAlbumDetailItemFactory = IoC.Container.Get<IArtistPanoramaAlbumDetailItemFactory>();
            _newPageInstance = true;
            this.Loaded += OnArtistPanoramaViewLoaded;
            InitializeComponent();
        }
     
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            ArtistPanoramaViewModel artistPanoramaViewModel = this.DataContext as ArtistPanoramaViewModel;
            if (artistPanoramaViewModel != null)
            {
                State[ArtistPanoramaViewCurrentArtist] = _artist;
                State[ArtistPanoramaViewItems] = _artistItems;                
                State[ArtistPanoramaViewActivePanelIndex] = artistPanoramaViewModel.CurrentArtistItemIndex;
            }

            base.OnNavigatedFrom(e);
        }

        private void OnArtistPanoramaViewLoaded(object sender, RoutedEventArgs e)
        {
            // the page is an humble object, and the navigatorService, its sole dependency.
            var navigator = IoC.Container.Get<INavigatorService>();
            navigator.ActivateNavigationService(NavigationService, true);

            if (DataContext == null)
            {
                var artistTicket = NavigationContext.QueryString["artistTicket"];
                var artistAlbumsTicket = NavigationContext.QueryString["albumsListTicket"];                

                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewCurrentArtist))
                {
                    _artist = (SynoItem)this.State[ArtistPanoramaViewCurrentArtist];
                }
                else
                {
                    _artist = (SynoItem)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(artistTicket);                    
                }

                IEnumerable<SynoItem> artistItems = null;
                IEnumerable<IAlbumViewModel> albumViewModels;
                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewItems))
                {
                    throw new NotImplementedException("deal with the viewmodels not with the synoitems anymore here");
                    artistItems = (IEnumerable<SynoItem>)this.State[ArtistPanoramaViewItems];
                    //artistPanoramaViewModel.BuildArtistItems(_artistItems);
                }
                else
                {
                    //artistPanoramaViewModel.QueryAndBuildArtistItems();
                    albumViewModels = (IEnumerable<IAlbumViewModel>)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(artistAlbumsTicket);
                }
                var albumTicket = NavigationContext.QueryString["albumTicket"];

                int artistPanoramaViewActivePanelIndex = 0;

                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewCurrentArtist))
                {
                    artistPanoramaViewActivePanelIndex = (int)this.State[ArtistPanoramaViewActivePanelIndex];
                }
                else
                {
                    var album = (IAlbumViewModel)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(albumTicket);
                    artistPanoramaViewActivePanelIndex = albumViewModels.ToList().IndexOf(album);




                }
                ArtistPanoramaViewModel artistPanoramaViewModel = IoC.Container.Get<ArtistPanoramaViewModelFactory>().Create(this._artist, albumViewModels,artistPanoramaViewActivePanelIndex);
                
                DataContext = artistPanoramaViewModel;
            }
        }

        private void ShowPlayQueue(object sender, EventArgs e)
        {
            var viewModel = (ArtistPanoramaViewModel)DataContext;
            viewModel.ShowPlayQueueCommand.Execute(null);
        }

        private void PlayLast(object sender, EventArgs e)
        {
            var viewModel = (ArtistPanoramaViewModel)DataContext;
            viewModel.PlayLastCommand.Execute(null);

        }
    }
}