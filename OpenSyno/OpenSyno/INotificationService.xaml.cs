namespace OpenSyno
{
    using System;

    public interface INotificationService
    {
        void Warning(string warningMessage, string warningTitle);

        void Error(string message, string messageTitle);
    }
}