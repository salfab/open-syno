namespace OpenSyno.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;

    public class IsolatedStorageLogService : ILogService
    {
        private IsolatedStorageFileStream _logFile;
        private StreamWriter _writer;
        private Dictionary<string,bool> _conditionalTracingActivations = new Dictionary<string, bool>();

        public IsolatedStorageLogService()
        {
            // by default, logging is enabled.
            IsEnabled = true;
            using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                _logFile = userStore.OpenFile("logfile.log", FileMode.Append, FileAccess.Write, FileShare.Read );                
            }
            _writer = new StreamWriter(_logFile);
            _writer.AutoFlush = true;
        }

        public void ActivateConditionalTracing(string key)
        {
            if (_conditionalTracingActivations.ContainsKey(key))
            {
                _conditionalTracingActivations[key] = true;            
            }
            else
            {
                _conditionalTracingActivations.Add(key, true);                
            }
        }

        public void DeactivateConditionalTracing(string key)
        {
            if (_conditionalTracingActivations.ContainsKey(key))
            {
                _conditionalTracingActivations[key] = false;
            }
            else
            {
                _conditionalTracingActivations.Add(key, false);
            }
        }

        public void ConditionalTrace(string message, string conditionKey)
        {
            if (_conditionalTracingActivations.ContainsKey(conditionKey) && _conditionalTracingActivations[conditionKey])
            {
                Trace(message);
            }
        }

        #region Implementation of ILogService

        public bool IsEnabled { get; set; }
        public void Trace(string message)
        {
            if (IsEnabled)
            {
                _writer.WriteLine("[{0}] - {1}", DateTime.Now, message);
            }
            
        }

        public void Error(string message)
        {
            if (IsEnabled)
            {
                _writer.WriteLine("ERROR : [{0}] - {1}", DateTime.Now, message);
            }
        }

        public string GetLogFile()
        {
            string log = string.Empty;
            if (IsEnabled)
            {
                IsEnabled = false;
                _logFile.Close();
                _logFile.Dispose();
                using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var logFile = userStore.OpenFile("logfile.log", FileMode.Open, FileAccess.Read))
                    {
                        var reader = new StreamReader(logFile);
                        log = reader.ReadToEnd();
                    }
                }
            }
            return log;            
        }

        public void ClearLog()
        {
            using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (userStore.FileExists("logfile.log"))
                {
                    userStore.DeleteFile("logfile.log");   
                }
            }
        }

        #endregion
    }
}
