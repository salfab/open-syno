using Synology.AudioStationApi;

namespace OpenSyno
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using OpemSyno.Contracts;

    using OpenSyno.ViewModels;

    public class AlbumViewModel : ViewModelBase, IAlbumViewModel
    {
        public AlbumViewModel(SynoItem album)
        {
            this.Album = album;
            SelectedCommand = new DelegateCommand(OnSelected);
            SelectAllOrNoneCommand = new DelegateCommand(OnSelectAllOrNone);
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

        public ObservableCollection<ITrackViewModel> Tracks { get; set; }

        public SynoItem Album { get; set; }

        public ICommand SelectedCommand { get; set; }

        public bool IsBusy { get; set; }
    }
}