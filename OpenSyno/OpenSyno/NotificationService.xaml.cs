using System;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;
using OpemSyno.Contracts.Services;
using OpenSyno.Services;

namespace OpenSyno
{
    using System.Windows;

    using OpemSyno.Contracts;

    public class NotificationService : INotificationService
    {
        private ILogService _logService;

        public NotificationService(ILogService logService)
        {
            _logService = logService;
        }

        public void Warning(string warningMessage, string warningTitle)
        {
            _logService.Trace(warningMessage);

            Action errorFeedback = () => MessageBox.Show(warningMessage, warningTitle, MessageBoxButton.OK);

            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(errorFeedback);
            }
            else
            {
                errorFeedback();
            }
            
        }

        public void Error(string message, string messageTitle)
        {
            var checkAccess = Deployment.Current.Dispatcher.CheckAccess();
            if (!checkAccess)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show(message, messageTitle, MessageBoxButton.OK));
            }
            else
            {
                MessageBox.Show(message, messageTitle, MessageBoxButton.OK);
            }
        }

        public MessageBoxResult WarningQuery(string warningMessage, string warningTitle, MessageBoxButton userResponseOptions)
        {
            return MessageBox.Show(warningMessage, warningTitle, userResponseOptions);
        }
    }
}