using System;
using System.Net;
using Microsoft.Phone.BackgroundAudio;
using OpenSyno.Services;

namespace OpenSyno.Common
{
    using System.Linq;
    using System.Threading;

    using Newtonsoft.Json.Linq;

    using Synology.AudioStationApi;

    public class AudioTrackFactory : IAudioTrackFactory
    {
        #region Implementation of IAudioTrackFactory

        public void BeginCreate(SynoTrack baseSynoTrack, Guid guid, string host, int port, string token, Action<AudioTrack> successCallback, Action<Exception> errorCallback)
        {
          
            // hack : Synology's webserver doesn't accept the + character as a space : it needs a %20, and it needs to have special characters such as '&' to be encoded with %20 as well, so an HtmlEncode is not an option, since even if a space would be encoded properly, an ampersand (&) would be translated into &amp;
            string url =
                string.Format(
                    "http://{0}:{1}/audio/webUI/audio_stream.cgi/0.mp3?sid={2}&action=streaming&songpath={3}",
                    host,
                    port,
                    token.Split('=')[1],
                    HttpUtility.UrlEncode(baseSynoTrack.Res).Replace("+", "%20"));

            if (baseSynoTrack.Res.Contains("&"))
            {
                try
                {
                    // Use url-shortening service.
                    // http://t0.tv/api/shorten?u=<url>
                    WebClient webClient = new WebClient();
                    var mre = new ManualResetEvent(false);
                    webClient.DownloadStringCompleted += (s, e) =>
                        {
                            try
                            {
                                string result = e.Result;
                                JObject jo = JObject.Parse(result);
                                url = jo["short_url"].Value<string>();
                                var audioTrack = new AudioTrack(
                                    new Uri(url),
                                    baseSynoTrack.Title,
                                    baseSynoTrack.Artist,
                                    baseSynoTrack.Album,
                                    new Uri(baseSynoTrack.AlbumArtUrl),
                                    guid.ToString(),
                                    EnabledPlayerControls.All);
                                successCallback(audioTrack);
                            }
                            catch (Exception exception)
                            {
                                errorCallback(exception);
                                throw;
                            }
                            
                        };
                    webClient.DownloadStringAsync(new Uri("http://t0.tv/api/shorten?u=" + HttpUtility.UrlEncode(url)));
                }
                catch (Exception e)
                {
                    errorCallback(e);
                }
            }
            else
            {
                var audioTrack = new AudioTrack(
                new Uri(url),
                baseSynoTrack.Title,
                baseSynoTrack.Artist,
                baseSynoTrack.Album,
                new Uri(baseSynoTrack.AlbumArtUrl),
                guid.ToString(),
                EnabledPlayerControls.All);
                successCallback(audioTrack);
            }
        }

        #endregion
    }
}