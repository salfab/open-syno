using Synology.AudioStationApi;

namespace OpenSyno
{
    public class SelectedArtistChangedAggregatedEvent
    {
        public SynoItem Artist { get; set; }
    }
}