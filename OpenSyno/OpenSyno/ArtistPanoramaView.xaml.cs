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
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistPanoramaView"/> class.
        /// </summary>
        public ArtistPanoramaView()
        {
            this.Loaded += OnArtistPanoramaViewLoaded;
            InitializeComponent();
        }

        private void OnArtistPanoramaViewLoaded(object sender, RoutedEventArgs e)
        {
            // the page is an humble object, and the navigatorService, its sole dependency.
            var navigator = IoC.Container.Get<INavigatorService>();
            navigator.ActivateNavigationService(NavigationService, true);

            if (DataContext == null)
            {
                var artistTicket = NavigationContext.QueryString["artistTicket"];
                SynoItem artist = (SynoItem)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(artistTicket);
                DataContext = IoC.Container.Get<ArtistPanoramaViewModelFactory>().Create(artist);
            }
        }

        private void ShowPlayQueue(object sender, EventArgs e)
        {
            var viewModel = (ArtistPanoramaViewModel)DataContext;
            viewModel.ShowPlayQueueCommand.Execute(null);
        }
    }
}