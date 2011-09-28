using System;
using Microsoft.Phone.BackgroundAudio;

namespace OpenSyno.Services
{
    using Synology.AudioStationApi;

    public interface IAudioTrackFactory
    {
        void BeginCreate(SynoTrack baseSynoTrack, Guid guid, string host, int port, string token, Action<AudioTrack> successCallback, Action<Exception> errorCallback);
    }
}