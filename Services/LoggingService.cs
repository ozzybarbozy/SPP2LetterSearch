using System;
using System.IO;

namespace SPP2LetterSearch.Services
{
    public class LoggingService
    {
        private readonly string _logPath;
        private readonly object _lockObject = new object();

        public LoggingService()
        {
            _logPath = Constants.LogFilePath;
            EnsureLogDirectory();
        }

        private void EnsureLogDirectory()
        {
            try
            {
                var dir = Path.GetDirectoryName(_logPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch { }
        }

        public void Log(string message)
        {
            try
            {
                lock (_lockObject)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var logMessage = $"[{timestamp}] {message}";
                    File.AppendAllText(_logPath, logMessage + Environment.NewLine);
                }
            }
            catch { }
        }

        public void LogError(string message, Exception ex = null)
        {
            var fullMessage = ex != null
                ? $"ERROR: {message} - {ex.GetType().Name}: {ex.Message}"
                : $"ERROR: {message}";
            Log(fullMessage);
        }

        public void ClearLog()
        {
            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(_logPath))
                    {
                        File.Delete(_logPath);
                    }
                }
            }
            catch { }
        }
    }
}
