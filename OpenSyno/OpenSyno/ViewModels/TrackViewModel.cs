using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using OpemSyno.Contracts;

namespace OpenSyno.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    [DataContract]
    public class TrackViewModel : ViewModelBase
    {
        private bool _isSelected;

        private readonly IAudioStationSession _session;
        private readonly IPageSwitchingService _pageSwitchingService;
        private readonly IUrlParameterToObjectsPlateHeater _urlParameterToObjectsPlateHeater;
        private readonly AlbumViewModelFactory _albumViewModelFactory;

        private ITrackViewModelFactory _trackViewModelFactory;
        private INotificationService _notificationService;

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

        public TrackViewModel(Guid guid, SynoTrack synoTrack, IPageSwitchingService pageSwitchingService, AlbumViewModelFactory albumViewModelFactory, IAudioStationSession session, IUrlParameterToObjectsPlateHeater urlParameterToObjectsPlateHeater, ITrackViewModelFactory trackViewModelFactory, INotificationService notificationService)
        {
            if (synoTrack == null)
            {
                throw new ArgumentNullException("synoTrack");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            _trackViewModelFactory = trackViewModelFactory;
            _notificationService = notificationService;

            if (albumViewModelFactory == null) throw new ArgumentNullException("albumViewModelFactory");

            Guid = guid;
            TrackInfo = synoTrack;

            NavigateToContainingAlbumCommand = new DelegateCommand(OnNavigateToContainingAlbum);

            this._session = session;
            this._urlParameterToObjectsPlateHeater = urlParameterToObjectsPlateHeater;
            _albumViewModelFactory = albumViewModelFactory;
            _albumViewModelFactory = albumViewModelFactory;
            _pageSwitchingService = pageSwitchingService;
        }

        private void OnNavigateToContainingAlbum()
        {
            if (string.IsNullOrEmpty(this.TrackInfo.Artist))
            {
                _notificationService.Warning("We could not find other songs of the same album.\r\n\r\nThe track selected looks like it is not well tagged. No information could be found about the artist performing it, therefore, we could not navigate to the containing album.\r\nPlease consider tagging your 'Unknown Artist' tracks", "Sorry...");
                return;
            }
            
            Task<IEnumerable<SynoItem>> searchArtistsTask = this._session.SearchArtistAsync(this.TrackInfo.Artist);
            searchArtistsTask.ContinueWith(
                task =>
                    {
                        var artist = task.Result.SingleOrDefault(a => a.Title == TrackInfo.Artist);

                        // TODO : check also that the artist name match !! otherwise, two albums might have the same name and still be two different albums.
                        if (artist == null)
                        {
                            throw new NotSupportedException("we could not find strictly one perfect match for artist '" + TrackInfo.Artist + "' (" + task.Result.First().ItemID + "). Maybe there are multiple artists with the same name in your library which might mean your library is corrupted. " + task.Result.Count() + " matches were found. The song the artist is supposed to have sung is '"+ TrackInfo.Title +"'.");
                        }

                        Task<IEnumerable<SynoItem>> searchAlbumsTask = this._session.GetAlbumsForArtistAsync(artist);
                        searchAlbumsTask.ContinueWith(t =>
                                                          {
                                                              var albums = t.Result;
                                                              var album = albums.SingleOrDefault(o => o.Title == this.TrackInfo.Album);
                                                              if (album == null)
                                                              {
                                                                    throw new NotSupportedException("we could not find strictly one perfect match for albums names. Maybe there are multiple albums with the same name for the same artist in your library which might mean your library is corrupted.");                                                                  
                                                              }

                                                              var albumsListTicket = Guid.NewGuid().ToString();
                                                              
                                                              // TODO : move those registrations within the page switching service.

                                                              // the artist whose page to show,
                                                              this._urlParameterToObjectsPlateHeater.RegisterObject(artist.ItemID, artist);
                                                              


                                                              List<AlbumViewModel> albumViewModels = new List<AlbumViewModel>();
                                                                                                                            
                                                              foreach (var item in albums)
                                                              {
                                                                  AlbumViewModel viewModel = this._albumViewModelFactory.Create(item);                                                                  
                                                                  albumViewModels.Add(viewModel);

                                                                  // populate the tracks for each album.
                                                                  Task<IEnumerable<SynoTrack>> getTracksForAlbumTask = _session.GetTracksForAlbumAsync(item);
                                                                  getTracksForAlbumTask.ContinueWith(tracks =>
                                                                      {
                                                                          var albumViewModel = albumViewModels.Single(o => o.Album == tracks.AsyncState);
                                                                          albumViewModel.Tracks.Clear();
                                                                          foreach (var synoTrack in getTracksForAlbumTask.Result)
                                                                          {
                                                                              albumViewModel.Tracks.Add(this._trackViewModelFactory.Create(Guid.NewGuid(), synoTrack, _pageSwitchingService));
                                                                          }
                                                                      });
                                                              }

                                                              // the album shown by default.
                                                              AlbumViewModel defaultAlbumViewModel = albumViewModels.Single(o => o.Album == album);

                                                              this._urlParameterToObjectsPlateHeater.RegisterObject(album.ItemID, defaultAlbumViewModel);

                                                              // the albums list.
                                                              this._urlParameterToObjectsPlateHeater.RegisterObject(albumsListTicket, albumViewModels);

                                                              this._pageSwitchingService.NavigateToArtistPanorama(artist.ItemID,album.ItemID, albumsListTicket);
                                                          }, TaskScheduler.FromCurrentSynchronizationContext());                        

                    },
                TaskScheduler.FromCurrentSynchronizationContext());

        }

        public ICommand NavigateToContainingAlbumCommand { get; set; }
    }
}