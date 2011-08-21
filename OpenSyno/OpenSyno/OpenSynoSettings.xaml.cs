using System;

namespace OpenSyno
{
    using System.Collections.Generic;

    public class OpenSynoSettings : IOpenSynoSettings
    {
        public string Token { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }
        
        public int Port { get; set; }

        public List<Playlist> Playlists { get; set; }

        public bool UseSsl { get; set; }
      
    }
}