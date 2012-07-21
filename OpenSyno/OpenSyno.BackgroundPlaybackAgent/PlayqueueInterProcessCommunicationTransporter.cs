using System.Collections.Generic;
using System.Runtime.Serialization;
using OpemSyno.Contracts.Domain;
using OpenSyno.Contracts.Domain;

namespace OpenSyno.BackgroundPlaybackAgent
{
    [DataContract]
    public class PlayqueueInterProcessCommunicationTransporter
    {
        public PlayqueueInterProcessCommunicationTransporter()
        {
            Mappings = new List<GuidToTrackMapping>();
        }
        [DataMember]
        public string Host { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public List<GuidToTrackMapping> Mappings { get; set; }
    }
}