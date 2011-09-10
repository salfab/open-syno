namespace Synology.AudioStationApi
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SynoItem 
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the album art URL.
        /// </summary>
        /// <value>The album art URL.</value>
        [DataMember]
        public string AlbumArtUrl { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The item icon.</value>
        [DataMember]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is container.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is container; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsContainer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is track.
        /// </summary>
        /// <value><c>true</c> if this instance is track; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsTrack { get; set; }

        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        /// <value>The item ID.</value>
        [DataMember]
        public string ItemID { get; set; }

        /// <summary>
        /// Gets or sets the item pid.
        /// </summary>
        /// <value>The item pid.</value>
        [DataMember]
        public string ItemPid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SynoItem"/> is support.
        /// </summary>
        /// <value><c>true</c> if support; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool Support { get; set; }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>The sequence.</value>
        [DataMember]
        public int Sequence { get; set; }
    }
}