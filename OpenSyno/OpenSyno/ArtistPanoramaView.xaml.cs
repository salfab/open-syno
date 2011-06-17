using System;
using System.Windows;
using Ninject;
using OpenSyno.Services;

namespace OpenSyno
{
    using System.Collections.Generic;

    using Microsoft.Phone.Controls;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public partial class ArtistPanoramaView : PhoneApplicationPage
    {
        private bool _newPageInstance = false;
        private const string ArtistPanoramaViewCurrentArtist = "ArtistPanoramaViewCurrentArtist";

        private SynoItem _artist;

        private IEnumerable<SynoItem> _artistItems;

        private const string ArtistPanoramaViewActivePanelIndex = "ArtistPanoramaViewActivePanelIndex";

        private const string ArtistPanoramaViewItems = "ArtistPanoramaViewItems";

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistPanoramaView"/> class.
        /// </summary>
        public ArtistPanoramaView()
        {
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

                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewCurrentArtist))
                {
                    _artist = (SynoItem)this.State[ArtistPanoramaViewCurrentArtist];
                }
                else
                {
                    _artist = (SynoItem)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(artistTicket);                    
                }

                int artistPanoramaViewActivePanelIndex = 0;
                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewCurrentArtist))
                {
                    artistPanoramaViewActivePanelIndex = (int)this.State[ArtistPanoramaViewActivePanelIndex];
                }

                ArtistPanoramaViewModel artistPanoramaViewModel = IoC.Container.Get<ArtistPanoramaViewModelFactory>().Create(this._artist);

                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewItems))
                {
                    _artistItems = (IEnumerable<SynoItem>)this.State[ArtistPanoramaViewItems];
                    artistPanoramaViewModel.BuildArtistItems(_artistItems);
                }
                else
                {
                    artistPanoramaViewActivePanelIndex = 0;
                    // FIXME : make it look more like a humble object.
                    // FIXME big time : what the heck is it doing in the view ? is there no other way to handle the tombstoning  ???
                    var searchService = IoC.Container.Get<ISearchService>();
                    searchService.GetAlbumsForArtist(_artist, (a, b, c) =>
                        {
                            _artistItems = a;
                            artistPanoramaViewModel.BuildArtistItems(a);
                        });
                }
                DataContext = artistPanoramaViewModel;
                artistPanoramaViewModel.CurrentArtistItemIndex = artistPanoramaViewActivePanelIndex;
                this.InvalidateArrange();
                this.InvalidateMeasure();

            }
        }

        private void ShowPlayQueue(object sender, EventArgs e)
        {
            var viewModel = (ArtistPanoramaViewModel)DataContext;
            viewModel.ShowPlayQueueCommand.Execute(null);
        }
    }
}