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
            string url =
                string.Format(
                    "http://{0}:{1}/audio/webUI/audio_stream.cgi/0.mp3?sid={2}&action=streaming&songpath={3}",
                    host,
                    port,
                    token.Split('=')[1],
                    HttpUtility.UrlEncode(baseSynoTrack.Res).Replace("+", "%20"));
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