using System;

namespace SpackiBot.Logging
{
    internal class LogMessage
    {
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public DateTime Timestamp { get; private set; }

        public LogMessage(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}