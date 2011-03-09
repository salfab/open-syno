using System;

namespace OpenSyno.Services
{
    public class TrackCurrentPositionChangedEventArgs
    {
        public TimeSpan Position { get; set; }

        public double PlaybackPercentComplete { get; set; }
        
        public double LoadPercentComplete { get; set; }

    }
}