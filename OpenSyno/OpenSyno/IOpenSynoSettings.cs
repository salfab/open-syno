namespace OpenSyno
{
    using System.Collections.Generic;

    using OpemSyno.Contracts;

    using OpenSyno.ViewModels;

    using Synology.AudioStationApi;

    public interface IOpenSynoSettings
    {        
        string Token { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string Host { get; set; }
        int Port { get; set; }

        List<Playlist> Playlists { get; set; }

        bool UseSsl { get; set; }

        DsmVersions DsmVersion { get; set; }

        CredentialFormatValidationResult IsCredentialFormatValid();
    }
}