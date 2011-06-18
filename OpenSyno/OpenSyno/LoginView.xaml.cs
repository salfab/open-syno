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
            // FIXME : Use a ViewModelResolver (Humble object)
            DataContext = IoC.Container.Get<LoginViewModel>();
        }

        private void ApplicationBarSignInButtonClicked(object sender, EventArgs e)
        {
            var focusedElement = (FrameworkElement)FocusManager.GetFocusedElement();
            AppBarBindingsHelper.UpdateBinding(focusedElement);

            var viewModel = (LoginViewModel)DataContext;
            viewModel.SignInCommand.Execute(null);
        }

        private void ApplicationBarLogsButtonClicked(object sender, EventArgs e)
        {
            // TODO : Move this to a proper command !
            ILogService logService = IoC.Container.Get<ILogService>();

            // TODO : Move this to a notification service
            MessageBox.Show("Logging will resume next time the application is started.", "Logs disabled", MessageBoxButton.OK);
            if (logService.IsEnabled)
            {
                // TODO : Move this to a notification service
                var result = MessageBox.Show(logService.GetLogFile(), "Click Cancel to clear logs", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    logService.ClearLog();
            }

        }
    }
}