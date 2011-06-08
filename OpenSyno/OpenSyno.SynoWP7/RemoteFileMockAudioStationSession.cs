namespace Synology.AudioStationApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    public class RemoteFileMockAudioStationSession : IAudioStationSession
    {
        /// <summary>
        /// Gets the remote file network stream.
        /// </summary>
        /// <param name="synoTrack">The track wor which to retrieve the stream.</param>
        /// <param name="callback">The method to call after the stream is open. The stream itself and its content length are passed as arguments.</param>
        /// <remarks>The caller is responsible for closing the stream after the call to DownloadFile returns</remarks>
        public void GetFileStream(SynoTrack synoTrack, Action<WebResponse, SynoTrack> callback)
        {
            if (synoTrack == null)
            {
                throw new ArgumentNullException("synoTrack");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            var client = new WebClient();
            var request = (HttpWebRequest)WebRequest.Create("http://www.toetapz.com/downloads/charge.mp3");
           

            //Set request headers.
            request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            //request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US");
            //request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; InfoPath.2; OfficeLiveConnector.1.5; OfficeLivePatch.1.3; Zune 4.7)";
            //request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            //request.Headers.Set(HttpRequestHeader.Cookie, @"__utma=11735858.713408819.1284879944.1294622128.1297023459.8; __utmz=11735858.1297023459.8.8.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=TiltEffect%20Toolkit; __qca=P0-1026483267-1284879945945");

            request.BeginGetResponse(OnFileDownloadResponseReceived, new FileDownloadResponseReceivedUserState(request, callback, synoTrack));
        }

        private void OnFileDownloadResponseReceived(IAsyncResult ar)
        {
            var userState = (FileDownloadResponseReceivedUserState)ar.AsyncState;

            WebResponse response = userState.Request.EndGetResponse(ar);

            userState.GetResponseCallback(response, userState.SynoTrack);
        }


        public void LoginAsync(string login, string password, Action<string> callback, Action<Exception> callbackError)
        {
            callback("#FAKETOKEN!");
        }

        public void SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>> callback, Action<Exception> callbackError)
        {
            throw new NotImplementedException();
        }

        public void SearchArtist(string pattern, Action<IEnumerable<SynoItem>> callback, Action<Exception> callbackError)
        {
            var results = new List<SynoItem>();

            results.Add(new SynoItem
            {
                Title = "Tom Waits",
                ItemPid = "musiclib_music_aa"
            });

            results.Add(new SynoItem
            {
                Title = "Mike Patton",
                ItemPid = "musiclib_music_aa"
            });

            results.Add(new SynoItem
            {
                Title = "65daysofstatic",
                ItemPid = "musiclib_music_aa"
            });

            callback(results);
        }

        public void GetAlbumsForArtist(SynoItem artist, Action<IEnumerable<SynoItem>, long, SynoItem> callback, Action<Exception> callbackError)
        {
            throw new NotImplementedException();
        }

        public void GetTracksForAlbum(SynoItem album, Action<IEnumerable<SynoTrack>, long, SynoItem> callback, Action<Exception> callbackError)
        {
            throw new NotImplementedException();
        }

        public bool IsSignedIn
        {
            get
            {
                return true;
            }
        }

        public string Host
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Port
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Token
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}