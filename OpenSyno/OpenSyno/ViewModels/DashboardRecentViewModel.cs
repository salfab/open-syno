namespace OpenSyno.ViewModels
{
    using System.Collections.Generic;

    using Synology.AudioStationApi;

    public class DashboardRecentViewModel
    {
        public List<SynoTrack> Tracks { get; set; }
    }
}