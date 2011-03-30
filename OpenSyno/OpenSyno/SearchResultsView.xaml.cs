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

using OpenSyno.Services;
using OpenSyno.ViewModels;

namespace OpenSyno
{
    public partial class SearchResultsView : PhoneApplicationPage
    {
        public SearchResultsView()
        {
            Loaded += PageLoaded;
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            // we use a factory so we can inject a PageSwitchingService in the view model.
            var searchResultsViewModelFactory = IoC.Current.Resolve<SearchResultsViewModelFactory>();

            // TODO : think about either block the UI while it's loading or not reloading the view model if it exists ( and therefore keep the same artists list as it was last time we loaded it.)
            DataContext = searchResultsViewModelFactory.Create(new PageSwitchingService(NavigationService));
        }
    }
}