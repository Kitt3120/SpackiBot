using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SpackiBot.Logging
{
    class Logger
    {
        private static List<LogLevel> Levels { get; } = new List<LogLevel>();

        public static void Debug(LoggingSection loggingSection, string message) => Log(loggingSection, LogLevel.Debug, message);
        public static void Verbose(LoggingSection loggingSection, string message) => Log(loggingSection, LogLevel.Verbose, message);
        public static void Info(LoggingSection loggingSection, string message) => Log(loggingSection, LogLevel.Info, message);
        public static void Warning(LoggingSection loggingSection, string message) => Log(loggingSection, LogLevel.Warning, message);
        public static void Error(LoggingSection loggingSection, string message) => Log(loggingSection, LogLevel.Error, message);
        public static void Critical(LoggingSection loggingSection, string message) => Log(loggingSection, LogLevel.Critical, message);

        static Logger()
        {
            foreach (LogLevel logLevel in Enum.GetValues(typeof(LogLevel)))
                Levels.Add(logLevel);
        }

        public static void Log(LoggingSection loggingSection, LogLevel logLevel, string message)
        {
            if (loggingSection.Disposed)
                return;

            LogMessage logMessage = new LogMessage(logLevel, message);
            loggingSection.History.Append(logMessage);
            if(Levels.Contains(logLevel))
                Console.WriteLine($"[{logLevel.ToString()}] - [{loggingSection.FullPath}]: {message}");
        }

        public static bool IsEnabled(LogLevel logLevel) => Levels.Contains(logLevel);
        public static void SetEnabled(LogLevel logLevel, bool enabled)
        {
            if (enabled)
            {
                if (IsEnabled(logLevel))
                    return;
                Levels.Add(logLevel);
            }
            else
                Levels.Remove(logLevel);
        }
    }
}
