using System;
using FlurryWP7SDK;

namespace OpenSyno.Services
{
    public class FlurryTrackerService : IFlurryTrackerService
    {
        private readonly string _apiKey;

        public FlurryTrackerService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void StartSession()
        {
            Api.StartSession(_apiKey);
        }

        public void LogError(string message, Exception exception)
        {
            Api.LogError(message, exception);            
        }
    }
}