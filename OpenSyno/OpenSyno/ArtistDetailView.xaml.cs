using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace OpenSyno
{
    using Ninject;

    using OpemSyno.Contracts;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public partial class ArtistDetailView : PhoneApplicationPage
    {
        public ArtistDetailView()
        {
            // Use a factory from the plate heater and use it to build the actual view model.
            // this.DataContext = IoC.Container.Get<IArtistDetailViewModel>();
            this.Loaded += OnLoaded;
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var navigator = IoC.Container.Get<INavigatorService>();
            var viewModelFactory = IoC.Container.Get<IArtistDetailViewModelFactory>();

            string ticket = this.NavigationContext.QueryString["artistTicket"];

            SynoItem artist = (SynoItem)navigator.UrlParameterToObjectsPlateHeater.GetObjectForTicket(ticket);
            
            DataContext = viewModelFactory.Create(artist);

        }
    }



    public class ArtistDetailViewModelFactory : IArtistDetailViewModelFactory
    {
        public IArtistDetailViewModel Create(SynoItem artist)
        {
            return new ArtistDetailViewModel(artist);
        }
    }
}
