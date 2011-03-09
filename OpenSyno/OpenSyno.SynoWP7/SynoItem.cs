namespace Synology.AudioStationApi
{
    public class SynoItem
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the album art URL.
        /// </summary>
        /// <value>The album art URL.</value>
        public string AlbumArtUrl { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The item icon.</value>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is container.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is container; otherwise, <c>false</c>.
        /// </value>
        public bool IsContainer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is track.
        /// </summary>
        /// <value><c>true</c> if this instance is track; otherwise, <c>false</c>.</value>
        public bool IsTrack { get; set; }

        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        /// <value>The item ID.</value>
        public string ItemID { get; set; }

        /// <summary>
        /// Gets or sets the item pid.
        /// </summary>
        /// <value>The item pid.</value>
        public string ItemPid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SynoItem"/> is support.
        /// </summary>
        /// <value><c>true</c> if support; otherwise, <c>false</c>.</value>
        public bool Support { get; set; }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>The sequence.</value>
        public int Sequence { get; set; }
    }
}