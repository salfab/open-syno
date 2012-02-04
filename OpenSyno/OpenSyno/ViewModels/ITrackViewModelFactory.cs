using System;

using OpenSyno.Services;
using Synology.AudioStationApi;

namespace OpenSyno.ViewModels
{
    public interface ITrackViewModelFactory
    {
        TrackViewModel Create(Guid guid, SynoTrack track, IPageSwitchingService pageSwitchingService);
    }
}