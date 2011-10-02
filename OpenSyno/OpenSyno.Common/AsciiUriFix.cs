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
        [DataMember]
        public string Url { get; set; }
    }
}