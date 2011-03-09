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
using Microsoft.Practices.Prism.Events;
using OpenSyno.Services;
using OpenSyno.ViewModels;

namespace OpenSyno
{
    using System;

    public partial class SearchView
    {
        public SearchView()
        {
            Loaded += PageLoaded;
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            // FIXME : Use IoC or a ViewModelLocator
            var eventAggregator = IoC.Current.Resolve<IEventAggregator>();
            DataContext = new SearchViewModel(IoC.Current.Resolve<ISearchService>(), new PageSwitchingService(NavigationService), eventAggregator);
        }

        /// <summary>
        /// TEMPORARY EVENT HANDLER JUST TO BE ABLE TO SET THE CREDENTIALS BEFORE WE PUT IT AT THE RIGHT PLACE IN A PROPER SETTINGS VIEW.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CredentialsApplicationBarButtonClicked(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoginView.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}