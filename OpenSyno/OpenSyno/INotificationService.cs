namespace OpenSyno
{
    using System;
    using System.Windows;

    public interface INotificationService
    {
        void Warning(string warningMessage, string warningTitle);

        void Error(string message, string messageTitle);

        MessageBoxResult WarningQuery(string warningMessage, string warningTitle, MessageBoxButton userResponseOptions);
    }
}