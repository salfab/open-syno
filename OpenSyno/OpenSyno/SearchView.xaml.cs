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
using Ninject;
using Ninject.Activation;
using Ninject.Planning.Bindings;
using OpenSyno.Helpers;
using OpenSyno.Services;
using OpenSyno.ViewModels;

namespace OpenSyno
{
    using System;

    public class ViewNames
    {
        public const string SearchView = "SearchView";
    }

    public partial class SearchView
    {
        public SearchView()
        {
            Loaded += PageLoaded;
            InitializeComponent();
        }

        // HACK : If NavigationService in silverlight was the same as in WPF, we wouldn't have to put this logic in the view : we could have it in a view factory and have it more decoupled, unfortunately
        // when navigating in a SL / WP7 application there is no way for us to control the lifecycle of the page being navigated to.
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            string viewName = ViewNames.SearchView;// "SearchView"; // GetType().FullName;

            // Don't register if it has been already registered in the past.
            if (IoC.Container.GetBindings(typeof(IPageSwitchingService)).Count(o => o.Metadata.Name == viewName) == 0)
            {
                // register the type binding...
                IoC.Container.Bind<IPageSwitchingService>().ToConstant(new PageSwitchingService(NavigationService)).Named(viewName);                
            }
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

        private void AboutBoxApplicationBarButtonClicked(object sender, EventArgs e)
        {
            var viewModel = (SearchViewModel)DataContext;
            viewModel.ShowAboutBoxCommand.Execute(null);
        }
    }
}