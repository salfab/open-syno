namespace OpenSyno
{
    using System;
    using System.Windows;

    public class NotificationService : INotificationService
    {
        public void Warning(string warningMessage, string warningTitle)
        {
            MessageBox.Show(warningMessage, warningTitle, MessageBoxButton.OK);
        }

        public void Error(string message, string messageTitle)
        {
            MessageBox.Show(message, messageTitle, MessageBoxButton.OK);
        }
    }
}