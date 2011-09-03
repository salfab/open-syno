namespace OpenSyno.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IAudioRenderingService
    {
        event EventHandler<MediaPositionChangedEventArgs> MediaPositionChanged;

        event EventHandler<MediaEndedEventArgs> MediaEnded;

        event EventHandler<BufferingProgressUpdatedEventArgs> BufferingProgressUpdated;

        event EventHandler<PlayBackStartedEventArgs> PlaybackStarted;

        void Pause();

        void Resume();

        double GetVolume();

        void SetVolume(double volume);

        void StreamTrack(ISynoTrack trackToPlay);

        void OnPlayqueueItemsChanged(IList newItems, IList oldItems);
    }
}