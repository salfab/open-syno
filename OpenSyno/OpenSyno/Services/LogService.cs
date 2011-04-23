using System;
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
    }
}
