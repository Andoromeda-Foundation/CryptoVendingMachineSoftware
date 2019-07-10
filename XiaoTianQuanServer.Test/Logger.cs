using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace XiaoTianQuanServer.Test
{
    class Logger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Disposable();
        }
    }
}
