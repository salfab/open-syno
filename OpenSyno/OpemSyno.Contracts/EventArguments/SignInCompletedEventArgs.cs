using System;

namespace OpenSyno.Services
{
    public class SignInCompletedEventArgs : EventArgs
    {
        public string Token { get; set; }

        public bool IsBusy { get; set; }
    }
}