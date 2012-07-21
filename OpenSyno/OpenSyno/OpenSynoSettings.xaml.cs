using System;

namespace OpenSyno
{
    using System.Collections.Generic;

    using OpemSyno.Contracts;

    using Synology.AudioStationApi;

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

        public OpenSynoSettings()
        {
            this.Playlists = new List<Playlist>();
            this.Port = 5000;
        }
        public DsmVersions DsmVersion { get; set; }

        public CredentialFormatValidationStatus GetCredentialFormatValidationStatus()
        {
            if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
            {
                return CredentialFormatValidationStatus.EmptyUsernamePassword;
            }

            if (string.IsNullOrWhiteSpace(this.Host))
            {
                return CredentialFormatValidationStatus.HostEmpty;
            }

            if (CheckHostnameContainsPort(this.Host))
            {
                return CredentialFormatValidationStatus.PortIncludedInHostname;
            }

            return CredentialFormatValidationStatus.Valid;
        }

        private bool CheckHostnameContainsPort(string hostname)
        {
            var isUrlBadFormat = hostname.Contains(":");            
            return isUrlBadFormat;
        }
      
    }
}