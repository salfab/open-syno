using System;
using System.Net;

namespace Synology.AudioStationApi
{
    public class FileDownloadResponseReceivedUserState
    {
        public HttpWebRequest Request { get; set; }

        public Action<WebResponse, SynoTrack> GetResponseCallback { get; set; }

        public SynoTrack SynoTrack { get; set; }

        public FileDownloadResponseReceivedUserState(HttpWebRequest request, Action<WebResponse, SynoTrack> getResponseCallback, SynoTrack synoTrack)
        {
            Request = request;
            GetResponseCallback = getResponseCallback;
            SynoTrack = synoTrack;
        }
    }
}