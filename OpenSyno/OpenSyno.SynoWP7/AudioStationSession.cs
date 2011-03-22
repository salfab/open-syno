using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using OpenSyno.SynoWP7;

namespace Synology.AudioStationApi
{
    public class AudioStationSession : IAudioStationSession
    {
        private string _host;
        private int _port;
        private string _token;


        /// <summary>
        /// Gets the remote file network stream.
        /// </summary>
        /// <param name="synoTrack">The track wor which to retrieve the stream.</param>
        /// <param name="callback">The method to call after the stream is open. The HttpResponse is passed as argument</param>
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
            string url = string.Format("http://{0}:{1}/audio/webUI/audio_stream.cgi/0.mp3?action=streaming&songpath={2}", _host, _port, HttpUtility.HtmlEncode(synoTrack.Res));
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(new Uri(url), _token);
            //Set request headers.
            request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            //request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US");
            //request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; InfoPath.2; OfficeLiveConnector.1.5; OfficeLivePatch.1.3; Zune 4.7)";
            //request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            //request.Headers.Set(HttpRequestHeader.Cookie, @"__utma=11735858.713408819.1284879944.1294622128.1297023459.8; __utmz=11735858.1297023459.8.8.utmcsr=google|utmccn=(organic)|utmcmd=organic|utmctr=TiltEffect%20Toolkit; __qca=P0-1026483267-1284879945945");
            
            request.AllowReadStreamBuffering = false;
            request.BeginGetResponse(OnFileDownloadResponseReceived, new FileDownloadResponseReceivedUserState(request, callback, synoTrack));
            
        }

        private void OnFileDownloadResponseReceived(IAsyncResult ar)
        {
            var userState = (FileDownloadResponseReceivedUserState)ar.AsyncState;

            WebResponse response = userState.Request.EndGetResponse(ar);

            userState.GetResponseCallback(response,userState.SynoTrack);
        }

        public void LoginAsync(string login, string password, string host, int port, Action<string> callback, Action<Exception> callbackError)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (password == null) throw new ArgumentNullException("password");
            if (host == null) throw new ArgumentNullException("host");

            _host = host;
            _port = port;

            WebClient client = new WebClient();
            
            Uri uri =
                new UriBuilder
                {
                    Host = _host,
                    Path = @"/webman/login.cgi",
                    Query = string.Format("username={0}&passwd={1}", login, password),
                    Port = _port
                }.Uri;
            client.DownloadStringCompleted += (sender, e) =>
                                                  {
                                                      if (e.Error != null)
                                                      {
                                                          MessageBox.Show(e.Error.Message);
                                                      }
                                                      else
                                                      {
                                                          string cookie = ((WebClient)(sender)).ResponseHeaders["Set-Cookie"].Split(';').Where(s => s.StartsWith("id=")).Single();
                                                          _token = cookie;
                                                          callback(cookie);    
                                                      }
                                                      
                                                  };
            client.DownloadStringAsync(uri);
        }

        public void SearchAllMusic(string pattern, Action<IEnumerable<SynoTrack>> callback, Action<Exception> callbackError)
        {
            throw new NotImplementedException();
        }

        public void SearchArtist(string pattern, Action<IEnumerable<SynoItem>> callback, Action<Exception> callbackError)
        {
            string urlBase = string.Format("http://{0}:{1}", _host, _port);
            var url = urlBase + "/audio/webUI/audio_browse.cgi";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            
            // Not supported yet, but that would decrease the bandwidth usage from 1.3 Mb to 83 Kb ... Pretty dramatic, ain't it ?
            //request.Headers["Accept-Encoding"] = "gzip, deflate";

            request.UserAgent = "OpenSyno";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(new Uri(url), _token);

            request.Method = "POST";

            int limit = 10000;
            string postString = string.Format(@"sort=title&dir=ASC&action=browse&target=musiclib_music_aa&server=musiclib_music_aa&category=&keyword={0}&start=0&limit={1}", pattern, limit);
            byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postString);
            
            request.BeginGetRequestStream(ar =>
                {
                    // Just make sure we retrieve the right web request : no access to modified closure.
                    HttpWebRequest webRequest = (HttpWebRequest)ar.AsyncState;

                    var requestStream = webRequest.EndGetRequestStream(ar);
                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();

                    request.BeginGetResponse(
                        responseAr =>
                            {
                                // Just make sure we retrieve the right web request : no access to modified closure.                        
                                var httpWebRequest = responseAr.AsyncState;

                                var webResponse = webRequest.EndGetResponse(responseAr);
                                var responseStream = webResponse.GetResponseStream();
                                var reader = new StreamReader(responseStream);
                                var content = reader.ReadToEnd();

                                long count;
                                IEnumerable<SynoItem> artists;
                                SynologyJsonDeserializationHelper.ParseSynologyArtists(content, out artists, out count, urlBase);



                                var isOnUiThread = Deployment.Current.Dispatcher.CheckAccess();
                                if (isOnUiThread)
                                {
                                    if (count > limit)
                                    {
                                        MessageBox.Show(string.Format("number of available artists ({0}) exceeds supported limit ({1})", count, limit));
                                    }
                                    callback(artists);                                    
                                }
                                else
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            if (count > limit)
                                            {
                                                MessageBox.Show(string.Format("number of available artists ({0}) exceeds supported limit ({1})", count, limit));
                                            }
                                            callback(artists);
                                        });
                                }
                            },
                        webRequest);
                },
                request);





            //WebClient wc = new WebClient();
             
            //Uri uri =
            //    new UriBuilder
            //    {
            //        Host = _host,
            //        Path = @"/webman/login.cgi",
            //        Query = string.Format("sort=title&dir=ASC&action=browse&target=musiclib_music_aa&server=musiclib_music_aa&category=&keyword={0}&start=0&limit=1000", pattern),
            //        Port = _port
            //    }.Uri;
            //wc.Headers["Cookie"] = _token;
            //wc.DownloadStringCompleted += (sender, ea) =>
            //                                  {
            //                                      if (ea.Error != null)
            //                                      {
            //                                          callbackError(ea.Error);
            //                                          return;
            //                                      }
            //                                      long count;
            //                                      IEnumerable<SynoItem> artists;
            //                                      SynologyJsonDeserializationHelper.ParseSynologyArtists(ea.Result,
            //                                                                                             out artists,
            //                                                                                             out count);
            //                                      callback(artists);
            //                                  };
            //wc.DownloadStringAsync(uri);
            //HttpWebResponse response = null;

            //try
            //{
            //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://hamilcar.serveftp.com:5000/audio/webUI/audio_browse.cgi");

            //    request.Accept = "*/*";
            //    request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US");
            //    request.Referer = "http://hamilcar.serveftp.com:5000/audio/";
            //    request.Headers.Add("x-requested-with", "XMLHttpRequest");
            //    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            //    request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            //    request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; InfoPath.2; OfficeLiveConnector.1.5; OfficeLivePatch.1.3; Zune 4.7)";
            //    request.Headers.Set(HttpRequestHeader.Pragma, "no-cache");
            //    request.Headers.Set(HttpRequestHeader.Cookie, @"ys-ActivedPlayerPanel=s%3Astream-grid; ys-grid-view=o%3Acolumns%3Da%253Ao%25253Aid%25253Dn%2525253A0%25255Ewidth%25253Dn%2525253A600%255Eo%25253Aid%25253Dn%2525253A1%25255Ewidth%25253Dn%2525253A205%255Eo%25253Aid%25253Dn%2525253A2%25255Ewidth%25253Dn%2525253A205%255Eo%25253Aid%25253Dn%2525253A3%25255Ewidth%25253Dn%2525253A50%25255Ehidden%25253Db%2525253A1%255Eo%25253Aid%25253Dn%2525253A4%25255Ewidth%25253Dn%2525253A205%255Eo%25253Aid%25253Dn%2525253A5%25255Ewidth%25253Dn%2525253A50%25255Ehidden%25253Db%2525253A1%255Eo%25253Aid%25253Dn%2525253A6%25255Ewidth%25253Dn%2525253A50%25255Ehidden%25253Db%2525253A1%255Eo%25253Aid%25253Dn%2525253A7%25255Ewidth%25253Dn%2525253A205%5Esort%3Do%253Afield%253Ds%25253Atitle%255Edirection%253Ds%25253AASC; id=rQQWQkkDR9i5U");

            //    request.Method = "POST";

            //    string postString = @"sort=title&dir=ASC&action=browse&target=musiclib_music_aa&server=musiclib_music_aa&category=&keyword=Nirvana&start=0&limit=50";
            //    byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postString);
            //    request.ContentLength = postBytes.Length;
            //    Stream stream = request.GetRequestStream();
            //    stream.Write(postBytes, 0, postBytes.Length);
            //    stream.Close();

            //    response = (HttpWebResponse)request.GetResponse();
            //}
            //catch (WebException e)
            //{
            //    if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
            //    else return false;
            //}
            //catch (Exception)
            //{
            //    if (response != null) response.Close();
            //    return false;
            //}

            //return true;

        }

        public void GetAlbumsForArtist(SynoItem artist, Action<IEnumerable<SynoItem>, long, SynoItem> callback, Action<Exception> callbackError)
        {
            string urlBase = string.Format("http://{0}:{1}", _host, _port);
            var url = urlBase + "/audio/webUI/audio_browse.cgi";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            // Not supported yet, but that would decrease the bandwidth usage from 1.3 Mb to 83 Kb ... Pretty dramatic, ain't it ?
            //request.Headers["Accept-Encoding"] = "gzip, deflate";

            request.UserAgent = "OpenSyno";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(new Uri(url), _token);

            request.Method = "POST";

            int limit = 10000;
            string postString = string.Format(@"action=browse&target={0}&server=musiclib_music_aa&category=&keyword=&start=0&sort=title&dir=ASC&limit={1}", HttpUtility.UrlEncode(artist.ItemID), limit);
            byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postString);

            request.BeginGetRequestStream(ar =>
            {
                // Just make sure we retrieve the right web request : no access to modified closure.
                HttpWebRequest webRequest = (HttpWebRequest)ar.AsyncState;

                var requestStream = webRequest.EndGetRequestStream(ar);
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                request.BeginGetResponse(
                    responseAr =>
                    {
                        // Just make sure we retrieve the right web request : no access to modified closure.                        
                        var httpWebRequest = responseAr.AsyncState;

                        var webResponse = webRequest.EndGetResponse(responseAr);
                        var responseStream = webResponse.GetResponseStream();
                        var reader = new StreamReader(responseStream);
                        var content = reader.ReadToEnd();

                        long count;
                        IEnumerable<SynoItem> albums;
                        SynologyJsonDeserializationHelper.ParseSynologyAlbums(content, out albums, out count, urlBase);



                        var isOnUiThread = Deployment.Current.Dispatcher.CheckAccess();
                        if (isOnUiThread)
                        {
                            if (count > limit)
                            {
                                MessageBox.Show(string.Format("number of available albums ({0}) exceeds supported limit ({1})", count, limit));
                            }
                            callback(albums, count, artist);
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                if (count > limit)
                                {
                                    MessageBox.Show(string.Format("number of available artists ({0}) exceeds supported limit ({1})", count, limit));
                                }
                                callback(albums, count, artist);
                            });
                        }
                    },
                    webRequest);
            },
                request);
        }

        public void GetTracksForAlbum(SynoItem album, Action<IEnumerable<SynoTrack>, long, SynoItem> callback, Action<Exception> callbackError)
        {
            string urlBase = string.Format("http://{0}:{1}", _host, _port);

            var url = urlBase + "/audio/webUI/audio_browse.cgi";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            // Not supported yet, but that would decrease the bandwidth usage from 1.3 Mb to 83 Kb ... Pretty dramatic, ain't it ?
            //request.Headers["Accept-Encoding"] = "gzip, deflate";

            request.UserAgent = "OpenSyno";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.SetCookies(new Uri(url), _token);

            request.Method = "POST";

            int limit = 10000;
            string postString = string.Format(@"action=browse&target={0}&server=musiclib_music_aa&category=&keyword=&start=0&sort=title&dir=ASC&limit={1}", HttpUtility.UrlEncode(album.ItemID), limit);
            byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postString);

            request.BeginGetRequestStream(ar =>
            {
                // Just make sure we retrieve the right web request : no access to modified closure.
                HttpWebRequest webRequest = (HttpWebRequest)ar.AsyncState;

                var requestStream = webRequest.EndGetRequestStream(ar);
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                request.BeginGetResponse(
                    responseAr =>
                    {
                        // Just make sure we retrieve the right web request : no access to modified closure.                        
                        var httpWebRequest = responseAr.AsyncState;

                        var webResponse = webRequest.EndGetResponse(responseAr);
                        var responseStream = webResponse.GetResponseStream();
                        var reader = new StreamReader(responseStream);
                        var content = reader.ReadToEnd();

                        long total;
                        IEnumerable<SynoTrack> tracks;
                        SynologyJsonDeserializationHelper.ParseSynologyTracks(content, out tracks, out total, urlBase);

                        tracks = tracks.OrderBy(o => o.Track);

                        var isOnUiThread = Deployment.Current.Dispatcher.CheckAccess();
                        if (isOnUiThread)
                        {
                            if (total > limit)
                            {
                                MessageBox.Show(string.Format("number of available albums ({0}) exceeds supported limit ({1})", total, limit));
                            }
                            callback(tracks, total, album);
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                if (total > limit)
                                {
                                    MessageBox.Show(string.Format("number of available artists ({0}) exceeds supported limit ({1})", total, limit));
                                }
                                callback(tracks, total, album);
                            });
                        }
                    },
                    webRequest);
            },
                request);
        }

        public bool IsSignedIn
        {
            get
            {
                return _token != null;
            }
        }
    }
}