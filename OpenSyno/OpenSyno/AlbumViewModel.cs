using Synology.AudioStationApi;

namespace OpenSyno
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using OpemSyno.Contracts;

    public class AlbumViewModel : IAlbumViewModel
    {
        public AlbumViewModel(SynoItem album)
        {
            this.Album = album;
            SelectedCommand = new DelegateCommand(OnSelected);
        }

        protected virtual void OnSelected()
        {
            if (Selected != null)
            {
                Selected(this, EventArgs.Empty);
            }
        }

        public event EventHandler Selected;

        public ObservableCollection<ITrackViewModel> Tracks { get; set; }

        public SynoItem Album { get; set; }

        public ICommand SelectedCommand { get; set; }

        public bool IsBusy { get; set; }
    }
}