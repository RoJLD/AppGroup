using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace AppGroup.Logging
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }

    public static class Logger
    {
        private static string? _logFilePath;
        private static LogLevel _minLogLevel = LogLevel.Debug;
        private static bool _logToFile = false;
        private static bool _logToDebug = true;
        private static readonly object _lock = new object();

        public static void Initialize()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AppGroup",
                "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"AppGroup_{DateTime.Now:yyyyMMdd}.log");
            _logToFile = true;
        }

        public static void Configure(LogLevel minLevel, bool logToFile, bool logToDebug)
        {
            _minLogLevel = minLevel;
            _logToFile = logToFile;
            _logToDebug = logToDebug;
            if (logToFile && string.IsNullOrEmpty(_logFilePath))
                Initialize();
        }

        public static void Trace(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Trace, message, caller, file, line);

        public static void Debug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Debug, message, caller, file, line);

        public static void Info(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Information, message, caller, file, line);

        public static void Warn(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Warning, message, caller, file, line);

        public static void Error(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Error, message, caller, file, line);

        public static void Critical(string message, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Critical, message, caller, file, line);

        public static void Error(Exception ex, [CallerMemberName] string caller = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
            => Log(LogLevel.Error, $"{ex.Message}\nStackTrace:\n{ex.StackTrace}", caller, file, line);

        private static void Log(LogLevel level, string message, string caller, string file, int line)
        {
            if (level < _minLogLevel) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string fileName = Path.GetFileName(file);
            string logEntry = $"[{timestamp}] [{level}] [{fileName}:{line}::{caller}] {message}";

            if (_logToDebug)
                System.Diagnostics.Debug.WriteLine(logEntry);

            if (_logToFile && !string.IsNullOrEmpty(_logFilePath))
            {
                lock (_lock)
                {
                    try
                    {
                        File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                    }
                    catch { }
                }
            }
        }
    }
}
