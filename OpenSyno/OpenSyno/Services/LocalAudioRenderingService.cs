namespace OpenSyno.Services
{
    using Synology.AudioStationApi;

    public class LocalAudioRenderingService : AudioRenderingService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAudioRenderingService"/> class.
        /// </summary>
        /// <param name="audioStationSession">The audio station session.</param>
        public LocalAudioRenderingService(IAudioStationSession audioStationSession)
            : base(audioStationSession)
        {
        }
    }
}