using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Xunit.DependencyInjection.Logging
{
    public class XunitTestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
        private readonly ITestOutputHelperAccessor _accessor;
        private readonly Func<string, LogLevel, bool> _filter;

        /// <summary>Log minLevel LogLevel.Information</summary>
        public XunitTestOutputLoggerProvider(ITestOutputHelperAccessor accessor) : this(accessor, (name, level) => level >= LogLevel.Information && level < LogLevel.None) { }

        public XunitTestOutputLoggerProvider(ITestOutputHelperAccessor accessor, Func<string, LogLevel, bool> filter)
        {
            _accessor = accessor;
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new XunitTestOutputLogger(_accessor, name, _filter));
    }
}
