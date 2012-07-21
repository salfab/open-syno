using Microsoft.Phone.BackgroundAudio;

namespace OpemSyno.Contracts.Services
{
    public interface ILastFmScrobblingService
    {
        void Scrobble(AudioTrack track);
    }
}