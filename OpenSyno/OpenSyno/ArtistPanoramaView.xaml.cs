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
    using Microsoft.Practices.Prism.Events;

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

        private readonly ISearchService _searchService;

        private readonly IEventAggregator _eventAggregator;

        private readonly INotificationService notificationService;

        private const string ArtistPanoramaViewActivePanelIndex = "ArtistPanoramaViewActivePanelIndex";

        private const string ArtistPanoramaViewItems = "ArtistPanoramaViewItems";

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistPanoramaView"/> class.
        /// </summary>
        public ArtistPanoramaView(ISearchService searchService, IEventAggregator eventAggregator, INotificationService notificationService)
        {
            this._searchService = searchService;
            _eventAggregator = eventAggregator;
            this.notificationService = notificationService;
            this.notificationService = notificationService;
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

                

                ArtistPanoramaViewModel artistPanoramaViewModel = IoC.Container.Get<ArtistPanoramaViewModelFactory>().Create(this._artist);

                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewItems))
                {
                    _artistItems = (IEnumerable<SynoItem>)this.State[ArtistPanoramaViewItems];
                    artistPanoramaViewModel.BuildArtistItems(_artistItems);
                }
                else
                {                    
                    // FIXME : make it look more like a humble object.
                    // FIXME big time : what the heck is it doing in the view ? is there no other way to handle the tombstoning  ???
                    // there should be a parameterless BuildArtistItems  which would retrieve the artistItems itself.
                    var searchService = IoC.Container.Get<ISearchService>();
                    searchService.GetAlbumsForArtist(_artist, (a, b, c) =>
                        {
                            _artistItems = a;
                            artistPanoramaViewModel.BuildArtistItems(a);
                        });
                }

                int artistPanoramaViewActivePanelIndex = 0;
                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewCurrentArtist))
                {
                    artistPanoramaViewActivePanelIndex = (int)this.State[ArtistPanoramaViewActivePanelIndex];
                }
                else
                {
                    var albumTicket = NavigationContext.QueryString["albumTicket"];
                    var albumsListTicket = NavigationContext.QueryString["albumsListTicket"];

                    var album = (IAlbumViewModel)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(albumTicket);
                    var albums = (IEnumerable<IAlbumViewModel>)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(albumTicket);
                    var artistPanoramaAlbumDetailItems = albums.Select(a => new ArtistPanoramaAlbumDetailItem(a.Album, this._searchService,this._eventAggregator,this.notificationService));
                    var artistItems = new ObservableCollection<ArtistPanoramaItemViewModel>();
                    foreach (var item in artistPanoramaAlbumDetailItems)
                    {
                        artistItems.Add(item);
                    }
                    artistPanoramaViewModel.ArtistItems = artistItems;
                    artistPanoramaViewActivePanelIndex = albums.ToList().IndexOf(album);
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

        private void PlayLast(object sender, EventArgs e)
        {
            var viewModel = (ArtistPanoramaViewModel)DataContext;
            ArtistPanoramaItemViewModel artistPanoramaItemViewModel = viewModel.ArtistItems[viewModel.CurrentArtistItemIndex];
            if (artistPanoramaItemViewModel.PanoramaItemKind ==  ArtistPanoramaItemKind.AlbumDetail)
            {
                ((ArtistPanoramaAlbumDetailItem)artistPanoramaItemViewModel).PlayListOperationCommand.Execute(null);                
            }
            else
            {
                // This should not even be available
                // FIXME : Remove this : the button should be grayed-out !
                MessageBox.Show("Play command not available in this context. sorry, we'll hide it for the final version ;)", "Known \"bug\"", MessageBoxButton.OK);
            }
        }
    }
}