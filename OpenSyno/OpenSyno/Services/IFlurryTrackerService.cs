using System;

namespace OpenSyno.Services
{
    public interface IFlurryTrackerService
    {
        void StartSession();
        void LogError(string message, Exception exception);
    }
}