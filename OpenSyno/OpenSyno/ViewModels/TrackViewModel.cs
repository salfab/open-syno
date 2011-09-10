namespace OpenSyno.ViewModels
{
    using System;
    using System.Runtime.Serialization;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    [DataContract]
    public class TrackViewModel : ViewModelBase
    {
        private bool _isSelected;
        private const string IsSelectedPropertyName = "IsSelected";
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

        public TrackViewModel(SynoTrack synoTrack)
        {
            if (synoTrack == null)
            {
                throw new ArgumentNullException("synoTrack");
            }

            TrackInfo = synoTrack;
        }
    }
}