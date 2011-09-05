using System;
using Microsoft.Phone.BackgroundAudio;

namespace OpenSyno.Services
{
    public interface IAudioTrackFactory
    {
        AudioTrack Create(ISynoTrack baseSynoTrack, Guid guid, string host, int port, string token);
    }
}