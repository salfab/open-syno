using System.Runtime.Serialization;

namespace OpemSyno.Contracts.Domain
{
    [DataContract]
    public class SettingsInterProcessCommunicationTransporter
    {
        [DataMember]
        public LastFmSettings LastFmSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the settings object is undefined.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has all its default values; otherwise, <c>false</c>.
        /// </value>        
        /// <remarks>Having isUndefined can be used by the instanciator to flag the instance typically when a deserialization fails and when the object has been built with its default settings. Object must be marked manually and IsUndefined will be false by default.</remarks>
        [DataMember]
        public bool IsUndefined { get; set; }

        public SettingsInterProcessCommunicationTransporter()
        {
            IsUndefined = false;
        }
    }
}