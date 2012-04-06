using Synology.AudioStationApi;

namespace OpenSyno
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using OpenSyno.ViewModels;

    public class AlbumViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public AlbumViewModel(SynoItem album)
        {
            this.Album = album;
            this.Tracks = new ObservableCollection<TrackViewModel>();
            SelectedCommand = new DelegateCommand(OnSelected);
            SelectAllOrNoneCommand = new DelegateCommand(OnSelectAllOrNone,() => this.Tracks.Count > 0);
        }

        private void OnSelectAllOrNone()
        {
            bool newIsSelectedValue = !this.Tracks.First().IsSelected;
            foreach (var track in Tracks)
            {
                track.IsSelected = newIsSelectedValue;
            }
        }

        protected virtual void OnSelected()
        {
            if (Selected != null)
            {
                Selected(this, EventArgs.Empty);
            }
        }

        public ICommand SelectAllOrNoneCommand { get; set; }

        public event EventHandler Selected;

        public ObservableCollection<TrackViewModel> Tracks { get; set; }

        public SynoItem Album { get; set; }

        public ICommand SelectedCommand { get; set; }

        public bool IsBusy { get; set; }
    }
}