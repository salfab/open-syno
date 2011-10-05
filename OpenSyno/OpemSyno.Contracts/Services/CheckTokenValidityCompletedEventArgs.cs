using System;
using System.Net;

namespace OpenSyno.Services
{
    public class CheckTokenValidityCompletedEventArgs : EventArgs
    {
        public bool IsValid { get; set; }

        public string Token { get; set; }

        public WebException Error { get; set; }
    }
}