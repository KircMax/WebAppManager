// Copyright (c) 2025, Siemens AG
//
// SPDX-License-Identifier: MIT
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
        private static object _lock = new object();

        private LogLevel _level;
        
        public LogLevel Level 
        { 
            get
            {
                lock (_lock)
                {
                    return _level;
                }
            }
            set
            {
                lock (_lock)
                {
                    _level = value;
                }
            }
        }

        public InMemoryLogSaver(LogLevel level)
        {
            _level = level;
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
