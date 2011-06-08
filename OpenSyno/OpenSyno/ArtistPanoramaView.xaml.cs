using System;
using System.Windows;
using Ninject;
using OpenSyno.Services;

namespace OpenSyno
{
    using Microsoft.Phone.Controls;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public partial class ArtistPanoramaView : PhoneApplicationPage
    {
        private bool _newPageInstance = false;
        private const string ArtistPanoramaViewDataContext = "ArtistPanoramaViewDataContext";

        private SynoItem _artist;
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
            State[ArtistPanoramaViewDataContext] = _artist;
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

                if (_newPageInstance && State.ContainsKey(ArtistPanoramaViewDataContext))
                {
                    _artist = (SynoItem)this.State[ArtistPanoramaViewDataContext];
                }
                else
                {
                    _artist = (SynoItem)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(artistTicket);                    
                }
                DataContext = IoC.Container.Get<ArtistPanoramaViewModelFactory>().Create(_artist);
            }
        }

        private void ShowPlayQueue(object sender, EventArgs e)
        {
            var viewModel = (ArtistPanoramaViewModel)DataContext;
            viewModel.ShowPlayQueueCommand.Execute(null);
        }
    }
}