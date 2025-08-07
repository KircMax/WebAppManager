using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemens.Simatic.S7.Webserver.API.WebApplicationManager.Utils
{
    public class InMemoryLogSaver : ILogger
    {
        private readonly string _logFilePath;
        private static object _lock = new object();

        public readonly LogLevel Level;

        public InMemoryLogSaver(LogLevel level)
        {
            Level = level;
            LogMessages = new List<string>();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= Level;
        }

        public List<string> LogMessages;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            if (formatter != null)
            {
                lock (_lock)
                {
                    var n = Environment.NewLine;
                    string exc = "";
                    if (exception != null) exc = n + exception.GetType() + ": " + exception.Message + n + exception.StackTrace + n;
                    string logText = logLevel.ToString() + ": " + DateTime.Now.ToString() + " " + formatter(state, exception) + n + exc;
                    LogMessages.Add(logText);
                }
            }
        }
    }
}
