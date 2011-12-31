using System;
using OpemSyno.Contracts;
using OpenSyno.Services;
using Synology.AudioStationApi;

namespace OpenSyno.ViewModels
{
    public interface ITrackViewModelFactory
    {
        ITrackViewModel Create(Guid guid, SynoTrack track, IPageSwitchingService pageSwitchingService);
    }
}