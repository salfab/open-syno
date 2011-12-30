namespace OpenSyno.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using OpemSyno.Contracts;

    using OpenSyno.Services;

    using Synology.AudioStationApi;

    [DataContract]
    public class TrackViewModel : ViewModelBase, ITrackViewModel
    {
        private bool _isSelected;

        private readonly IAudioStationSession _session;

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

        public TrackViewModel(Guid guid, SynoTrack synoTrack, IAudioStationSession session)
        {
            if (synoTrack == null)
            {
                throw new ArgumentNullException("synoTrack");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            Guid = guid;
            TrackInfo = synoTrack;

            NavigateToContainingAlbumCommand = new DelegateCommand(OnNavigateToContainingAlbum);

            this._session = session;
        }

        private void OnNavigateToContainingAlbum()
        {                        
            Task<IEnumerable<SynoItem>> itemsTask = this._session.SearchAlbums(this.TrackInfo.Album);
            itemsTask.ContinueWith(
                task =>
                    {
                        var album = task.Result.SingleOrDefault(a => a.Title == TrackInfo.Album);

                        // TODO : check also that the artist name match !! otherwise, two albums might have the same name and still be two different albums.
                        if (album == null)
                        {
                            throw new NotSupportedException("we could not find strictly one perfect match for album names. This is not supported yet, but we'll work on it ;)");
                        }
                    },
                TaskScheduler.FromCurrentSynchronizationContext());

        }

        public ICommand NavigateToContainingAlbumCommand { get; set; }
    }
}