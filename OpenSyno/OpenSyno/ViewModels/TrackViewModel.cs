namespace OpenSyno.ViewModels
{
    using System;
    using System.Runtime.Serialization;

    using OpemSyno.Contracts;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    [DataContract]
    public class TrackViewModel : ViewModelBase, ITrackViewModel
    {
        private bool _isSelected;
        private const string IsSelectedPropertyName = "IsSelected";

        [DataMember]
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets a guid which is the same across consecutive tracks that are part of the same album.
        /// </summary>
        /// <remarks>This property can be used to group tracks by album *ONLY* if they are adjacent in the playqueue.</remarks>
        /// <value>The consecutive album identifier.</value>
        [DataMember]
        public Guid ConsecutiveAlbumIdentifier { get; set; }

        [DataMember]
        public SynoTrack TrackInfo { get; set; }

        [DataMember]
        public bool IsSelected
        {
            get 
            {
                return _isSelected;
            }
            set 
            {
                _isSelected = value;
                OnPropertyChanged(IsSelectedPropertyName);
            }
        }

        public TrackViewModel(Guid guid, SynoTrack synoTrack)
        {
            if (synoTrack == null)
            {
                throw new ArgumentNullException("synoTrack");
            }

            Guid = guid;
            TrackInfo = synoTrack;
        }
    }
}