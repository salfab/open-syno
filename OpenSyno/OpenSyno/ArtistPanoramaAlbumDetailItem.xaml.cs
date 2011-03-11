using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Synology.AudioStationApi;

namespace OpenSyno
{
    using System.Windows;

    public class ArtistPanoramaAlbumDetailItem : ArtistPanoramaItem
    {
        private readonly ISearchService _searchService;

        public SynoItem AlbumItemInfo { get; set; }

        public ICommand SelectAllOrNoneCommand { get; set; }

        public ICommand PlayListOperationCommand { get; set; }

        private ObservableCollection<TrackViewModel> _tracks;
        private IEventAggregator _eventAggregator;

        public ObservableCollection<TrackViewModel> Tracks
        {
            get
            {
                return _tracks;
            }
            set
            {
                _tracks = value;
            }
        }

        public ArtistPanoramaAlbumDetailItem(SynoItem album, ISearchService searchService, IEventAggregator eventAggregator) : base(ArtistPanoramaItemKind.AlbumDetail)
        {
            this._searchService = searchService;
            _eventAggregator = eventAggregator;
            AlbumItemInfo = album;
            Header = album.Title;

            Tracks = new ObservableCollection<TrackViewModel>();
            
            // TODO : List tracks of the album.
            _searchService.GetTracksForAlbum(album, GetTracksForAlbumCompleted);

            SelectAllOrNoneCommand = new DelegateCommand(OnSelectAllOrNone);
            PlayListOperationCommand = new DelegateCommand(OnPlayListOperation);
        }

        private void OnPlayListOperation()
        {
            var operation = PlayListOperation.Append;
            IEnumerable<TrackViewModel> selectedItems = Tracks.Where(o=> o.IsSelected);
            if (selectedItems.Count() < 1)
            {
                // FIXME : Use a service to de-couple the viewmodel and the way a message is visually shown to the user. (i.e. use a service)
                MessageBox.Show("Please, select at least one track to play.");
            }
            else
            {
                _eventAggregator.GetEvent<CompositePresentationEvent<PlayListOperationAggregatedEvent>>().Publish(new PlayListOperationAggregatedEvent(operation, selectedItems));                
            }
        }

        private void OnSelectAllOrNone()
        {
            if (Tracks.Count > 0)
            {
                var newSelectionState = !Tracks.First().IsSelected;
                foreach (var track in Tracks)
                {
                    track.IsSelected = newSelectionState;
                }
            }
        }

        private void GetTracksForAlbumCompleted(IEnumerable<SynoTrack> tracks, long l, SynoItem arg3)
        {                            
            foreach(var track in tracks.Select(o => new TrackViewModel(o)))
            {
                Tracks.Add(track);
            }
        }
    }
}