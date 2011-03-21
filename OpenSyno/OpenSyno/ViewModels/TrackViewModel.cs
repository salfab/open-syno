namespace OpenSyno.ViewModels
{
    using System;

    using Synology.AudioStationApi;

    public class TrackViewModel : ViewModelBase
    {
        private bool _isSelected;
        private const string IsSelectedPropertyName = "IsSelected";
        public SynoTrack TrackInfo { get; set; }

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