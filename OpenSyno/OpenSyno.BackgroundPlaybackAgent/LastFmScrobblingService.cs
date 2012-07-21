using System;
using Microsoft.Phone.BackgroundAudio;
using Ninject;
using OpemSyno.Contracts.Domain;
using OpemSyno.Contracts.Services;

namespace OpenSyno.BackgroundPlaybackAgent
{
    public class LastFmScrobblingService : ILastFmScrobblingService
    {
        private ILogService _logService;


        public LastFmScrobblingService(LastFmSettings lastFmSettings)
        {
            //this._logService = IoC.Container.Get<ILogService>();
        }

        public void Scrobble(AudioTrack track)
        {
            //_logService.Trace(string.Format("Scrobbling track {0} by {1}", track.Title, track.Artist));
        }
    }
}