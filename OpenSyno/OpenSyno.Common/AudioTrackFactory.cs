using System;
using System.Net;
using Microsoft.Phone.BackgroundAudio;
using OpenSyno.Services;

namespace OpenSyno.Common
{
    using Synology.AudioStationApi;

    public class AudioTrackFactory : IAudioTrackFactory
    {
        #region Implementation of IAudioTrackFactory

        public AudioTrack Create(SynoTrack baseSynoTrack, Guid guid, string host, int port, string token)
        {
            // hack : Synology's webserver doesn't accept the + character as a space : it needs a %20, and it needs to have special characters such as '&' to be encoded with %20 as well, so an HtmlEncode is not an option, since even if a space would be encoded properly, an ampersand (&) would be translated into &amp;
            string url =
                string.Format(
                    "http://{0}:{1}/audio/webUI/audio_stream.cgi/0.mp3?sid={2}&action=streaming&songpath={3}",
                    host,
                    port,
                    token.Split('=')[1],
                    HttpUtility.UrlEncode(baseSynoTrack.Res).Replace("+", "%20").Replace("&", "%26"));
            return new AudioTrack(
                new Uri(url),
                baseSynoTrack.Title,
                baseSynoTrack.Artist,
                baseSynoTrack.Album,
                new Uri(baseSynoTrack.AlbumArtUrl),
                guid.ToString(),
                EnabledPlayerControls.All);
        }

        #endregion
    }
}