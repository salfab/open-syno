using System;

namespace OpenSyno
{
    public class OpenSynoSettings : IOpenSynoSettings
    {
        public string Token { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }
        
        public int Port { get; set; }
    }
}