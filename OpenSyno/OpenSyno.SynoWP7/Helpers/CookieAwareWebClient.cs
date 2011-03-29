namespace Synology.AudioStationApi
{
    using System;
    using System.Net;
    using System.Security;

    public class CookieAwareWebClient : WebClient
    {

        [SecuritySafeCritical]
        public CookieAwareWebClient()
        {
            
        }

        private CookieContainer m_container = new CookieContainer();

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </returns>
        /// <param name="address">A <see cref="T:System.Uri"/> that identifies the resource to request.</param>
        public new HttpWebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = m_container;
            }

            return (HttpWebRequest)request;
        }
    }
}