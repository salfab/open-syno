using System;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
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