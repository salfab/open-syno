namespace OpenSyno
{
    using System.Windows;

    using OpemSyno.Contracts;

    public class NotificationService : INotificationService
    {
        public void Warning(string warningMessage, string warningTitle)
        {
            MessageBox.Show(warningMessage, warningTitle, MessageBoxButton.OK);
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