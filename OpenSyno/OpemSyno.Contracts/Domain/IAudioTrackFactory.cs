using System;
using Microsoft.Phone.BackgroundAudio;

namespace OpenSyno.Services
{
    using Synology.AudioStationApi;

    public interface IAudioTrackFactory
    {
        AudioTrack Create(SynoTrack baseSynoTrack, Guid guid, string host, int port, string token);
    }
}