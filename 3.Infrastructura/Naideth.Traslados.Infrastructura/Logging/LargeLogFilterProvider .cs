using Microsoft.Extensions.Logging;

namespace Naideth.Traslados.Infrastructura.Logging
{
    public class LargeLogFilterProvider : ILoggerProvider
    {
        private readonly ILoggerProvider _loggerProvider;

        public LargeLogFilterProvider(ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LargeLogFilter(_loggerProvider.CreateLogger(categoryName));
        }

        public void Dispose()
        {
            _loggerProvider?.Dispose();
        }

        private class LargeLogFilter : ILogger
        {
            private readonly ILogger _innerLogger;
            //private const int MaxLogSize = 15 * 1024 * 1024; // 15MB
            private const int MaxLogSize = 10 * 1024; // 900 KB (menos de 1 MB)

            public LargeLogFilter(ILogger innerLogger)
            {
                _innerLogger = innerLogger;
            }

            public IDisposable BeginScope<TState>(TState state) => _innerLogger.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (formatter == null) throw new ArgumentNullException(nameof(formatter));

                var message = formatter(state, exception);

                if (message.Length > MaxLogSize)
                {
                    var parts = SplitLargeLog(message);
                    foreach (var part in parts)
                    {
                        _innerLogger.Log(logLevel, eventId, part, exception, (s, e) => s);
                    }
                }
                else
                {
                    _innerLogger.Log(logLevel, eventId, state, exception, formatter);
                }
            }

            private List<string> SplitLargeLog(string log)
            {
                var logs = new List<string>();
                int index = 0;
                while (index < log.Length)
                {
                    int length = Math.Min(MaxLogSize, log.Length - index);
                    logs.Add(log.Substring(index, length));
                    index += length;
                }
                return logs;
            }
        }
   
    
    }
     

}
