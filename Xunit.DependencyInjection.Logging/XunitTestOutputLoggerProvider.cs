using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Xunit.DependencyInjection.Logging
{
    public class XunitTestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new ConcurrentDictionary<string, ILogger>();
        private readonly ITestOutputHelperAccessor _accessor;

        public XunitTestOutputLoggerProvider(ITestOutputHelperAccessor accessor) => _accessor = accessor;

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new XunitTestOutputLogger(_accessor, name));
    }
}
