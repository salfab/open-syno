namespace OpemSyno.Contracts.Services
{
    public interface ILogService
    {
        bool IsEnabled { get; set; }
        void Trace(string message);
        void Error(string message);
        string GetLogFile();
        void ClearLog();
        void ActivateConditionalTracing(string key);
        void DeactivateConditionalTracing(string key);
        void ConditionalTrace(string message, string conditionKey);
        string GetLogFileSinceAppStart();
        void Warning(string message);
    }
}