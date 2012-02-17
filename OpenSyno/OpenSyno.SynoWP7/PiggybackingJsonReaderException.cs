namespace Synology.AudioStationApi
{
    using System;

    using Newtonsoft.Json;

    public class PiggybackingJsonReaderException : Exception
    {
        public PiggybackingJsonReaderException(string message, JsonReaderException innerException)
            : base(message, innerException)
        {
        }
    }
}