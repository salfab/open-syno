using System;
using System.Runtime.Serialization;

namespace OpenSyno.Common
{
    [DataContract]
    public class AsciiUriFix
    {
        public AsciiUriFix(string res, string url)
        {
            Res = res;
            Url = url;
        }

        [DataMember]
        public string Res { get; set; }

        private string _url;

        [DataMember]
        public string Url
        {
            get { return _url; }
            set
            {
                var oldUrl = _url;
                _url = value;
                if (oldUrl == null && LoadingComplete != null)
                {
                    LoadingComplete(this, EventArgs.Empty);
                }

            }
        }

        public event EventHandler LoadingComplete;
    }
}