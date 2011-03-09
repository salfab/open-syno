using System;
using System.Windows;
using OpenSyno.Services;

namespace OpenSyno

        private void OnArtistPanoramaViewLoaded(object sender, RoutedEventArgs e)
        {
            var factory = IoC.Current.Resolve<ArtistPanoramaViewModelFactory>();
            DataContext = factory.Create(new PageSwitchingService(NavigationService));
        }
    }