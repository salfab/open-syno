using System;

namespace OpenSyno
{
    using System.Collections.Generic;

    using OpemSyno.Contracts;

    public class OpenSynoSettings : IOpenSynoSettings
    {
        public string Token { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }
        
        public int Port { get; set; }

        public List<Playlist> Playlists { get; set; }

        public Guid CurrentPlaylistGuid { get; set; }

        public bool UseSsl { get; set; }

        public CredentialFormatValidationResult IsCredentialFormatValid()
        {
            if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
            {
                return CredentialFormatValidationResult.EmptyUsernamePassword;
            }

            if (string.IsNullOrWhiteSpace(this.Host))
            {
                return CredentialFormatValidationResult.HostEmpty;
            }

            if (CheckHostnameContainsPort(this.Host))
            {
                return CredentialFormatValidationResult.PortIncludedInHostname;
            }

            return CredentialFormatValidationResult.Valid;
        }

        private bool CheckHostnameContainsPort(string hostname)
        {
            var isUrlBadFormat = hostname.Contains(":");            
            return isUrlBadFormat;
        }
      
    }
}