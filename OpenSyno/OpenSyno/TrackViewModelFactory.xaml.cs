using System;

using OpenSyno.Services;
using OpenSyno.ViewModels;
using Synology.AudioStationApi;

namespace OpenSyno
{
    public class TrackViewModelFactory : ITrackViewModelFactory
    {
        private readonly IAudioStationSession _session;
        private readonly IUrlParameterToObjectsPlateHeater _urlParameterToObjectsPlateHeater;
        private readonly AlbumViewModelFactory _albumViewModelFactory;


        public TrackViewModelFactory(IAudioStationSession session, IUrlParameterToObjectsPlateHeater urlParameterToObjectsPlateHeater, AlbumViewModelFactory albumViewModelFactory)
        {
            if (session == null) throw new ArgumentNullException("session");
            if (urlParameterToObjectsPlateHeater == null)
                throw new ArgumentNullException("urlParameterToObjectsPlateHeater");
            if (albumViewModelFactory == null) throw new ArgumentNullException("albumViewModelFactory");
            this._session = session;
            this._urlParameterToObjectsPlateHeater = urlParameterToObjectsPlateHeater;
            _albumViewModelFactory = albumViewModelFactory;
        }

        public TrackViewModel Create(Guid guid, SynoTrack track, IPageSwitchingService pageSwitchingService)
        {
            return new TrackViewModel(guid, track, pageSwitchingService, _albumViewModelFactory, _session, _urlParameterToObjectsPlateHeater, this);
        }
    }
}