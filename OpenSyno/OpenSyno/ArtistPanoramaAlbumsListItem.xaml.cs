using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using System.Windows.Navigation;

    using OpenSyno.Services;

    public class ArtistPanoramaAlbumsListItem : ArtistPanoramaItem
    {
        private readonly PageSwitchingService _pageSwitchingService;

        private ObservableCollection<AlbumViewModel> _albums;
        private string AlbumsPropertyName = "Albums";

        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set
            {
                _albums = value;
                OnPropertyChanged(AlbumsPropertyName);
            }
        }

        public ICommand ShowPlayQueueCommand { get; set; }

        public ArtistPanoramaAlbumsListItem(IEnumerable<SynoItem> albums, SynoItem artist, PageSwitchingService pageSwitchingService)
            : base(ArtistPanoramaItemKind.AlbumsList)
        {
            if (albums == null)
            {
                throw new ArgumentNullException("albums");
            }

            if (artist == null)
            {
                throw new ArgumentNullException("artist");
            }

            if (pageSwitchingService == null)
            {
                throw new ArgumentNullException("pageSwitchingService");
            }

            _pageSwitchingService = pageSwitchingService;

            // todo : localize
            Header = "Albums";
            
            Albums = new ObservableCollection<AlbumViewModel>();
            foreach(var albumViewModel in albums.Select(o => new AlbumViewModel(o)))
            {
                Albums.Add(albumViewModel);
            }

            ShowPlayQueueCommand = new DelegateCommand(OnShowPlayQueue);

        }

        /// <summary>
        /// Called when a request to navigate to the play queue view is requested.
        /// </summary>
        private void OnShowPlayQueue()
        {
            _pageSwitchingService.NavigateToPlayQueue();
        }
    }
}