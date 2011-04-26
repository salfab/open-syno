using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace OpenSyno.Helpers
{
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
            throw new NotImplementedException();
        }

        public string GetLogFile()
        {
            string log;
            MessageBox.Show("Logging will resume at next application startup.","Logs disabled",MessageBoxButton.OK);
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
    }
}
