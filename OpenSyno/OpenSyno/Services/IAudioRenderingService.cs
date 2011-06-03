namespace OpenSyno.Services
{
    using System;
    using System.IO;

    using Synology.AudioStationApi;

    public interface IAudioRenderingService
    {
        event EventHandler<MediaPositionChangedEventArgs> MediaPositionChanged;

        event EventHandler<MediaEndedEventArgs> MediaEnded;

        event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;

        event EventHandler<PlayBackStartedEventArgs> PlaybackStarted;

        void Bufferize(Action<Stream> bufferizedCallback, Action<double> bufferizeProgressChangedCallback , SynoTrack track);

        void Play(Stream trackStream);

        void Pause();

        void Resume();
    }
}