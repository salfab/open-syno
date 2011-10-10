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

    public class ArtistPanoramaAlbumsListItemViewModel : ArtistPanoramaItemViewModel
    {
        private readonly IPageSwitchingService _pageSwitchingService;

        private readonly IPanoramaItemSwitchingService _panoramaItemSwitchingService;

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

        public ICommand ShowAlbumDetailCommand { get; set; }

        public ArtistPanoramaAlbumsListItemViewModel(IEnumerable<SynoItem> albums, SynoItem artist, IPageSwitchingService pageSwitchingService, IPanoramaItemSwitchingService panoramaItemSwitchingService)
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
            _panoramaItemSwitchingService = panoramaItemSwitchingService;

            // todo : localize
            Header = "Albums";
            
            Albums = new ObservableCollection<AlbumViewModel>();
            foreach(var albumViewModel in albums.Select(o => new AlbumViewModel(o)))
            {
                Albums.Add(albumViewModel);
            }

            IsBusy = false;

            ShowPlayQueueCommand = new DelegateCommand(OnShowPlayQueue);
            ShowAlbumDetailCommand = new DelegateCommand<AlbumViewModel>(OnShowAlbumDetail);
        }

        private void OnShowAlbumDetail(AlbumViewModel albumViewModel)
        {
            // HACK : Sice we moved manually the "All Music" album at the end of the list (for ease of navigation), there's an offset which is compensated by the albums page. side effect is : we cannot jump to the all music item... either we'll have to special case the all music album, or we'll have to edit the "Albums" collection property so the All Music comes at the end of the list and place an offset of +1 when computing the item to jump to.

            var indexOf = this.Albums.IndexOf(albumViewModel);
            if (indexOf == -1)
            {
                // FIXME : Because of our templates in the artist panorama items, ( ViewModelProxy ) the Albums collection is not properly refreshed after it has been loaded once.
                // (blame it on silverlight 3 : no proper element name, no FindAncestor on bindings ; no dynamic resources...
                // we'll have to design our template differently :(
                // in the mean time, it will remain a known bug.
                return;
            }
            _panoramaItemSwitchingService.RequestActiveItemChange(indexOf);
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