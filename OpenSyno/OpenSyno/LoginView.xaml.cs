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
using OpenSyno.Helpers;
using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using System;
    using System.Windows.Navigation;

    public partial class LoginView
    {
        public LoginView()
        {
            Loaded += PageLoaded;
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            // FIXME : Use and instanciate a LoginViewModelFactory at application startup and use it to create the LoginViewModelFactory.
            var eventAggregator = IoC.Container.Resolve<IEventAggregator>();
            // FIXME : Use a factory.Create, and instanciate AudioStationSession and event aggregators at application startup
            DataContext = new LoginViewModel(new PageSwitchingService(NavigationService), eventAggregator, IoC.Container.Resolve<IAudioStationSession>(), IoC.Container.Resolve<IOpenSynoSettings>());
        }

        private void ApplicationBarSignInButtonClicked(object sender, EventArgs e)
        {
            var focusedElement = (FrameworkElement)FocusManager.GetFocusedElement();
            AppBarBindingsHelper.UpdateBinding(focusedElement);

            var viewModel = (LoginViewModel)DataContext;
            viewModel.SignInCommand.Execute(null);
        }
    }
}