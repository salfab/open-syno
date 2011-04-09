using System;
using System.Windows;
using Ninject;
using OpenSyno.Services;

namespace OpenSyno
{
    using Microsoft.Phone.Controls;

    using OpenSyno.ViewModels;

    public partial class ArtistPanoramaView : PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistPanoramaView"/> class.
        /// </summary>
        public ArtistPanoramaView()
        {
            this.Loaded += new System.Windows.RoutedEventHandler(OnArtistPanoramaViewLoaded);
            InitializeComponent();
        }

        private void OnArtistPanoramaViewLoaded(object sender, RoutedEventArgs e)
        {
            var factory = IoC.Container.Get<ArtistPanoramaViewModelFactory>();
            DataContext = factory.Create(new PageSwitchingService(NavigationService));
        }
    }
}