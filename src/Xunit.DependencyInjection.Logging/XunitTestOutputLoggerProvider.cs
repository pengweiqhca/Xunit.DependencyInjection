using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xunit.DependencyInjection.Logging;

[Obsolete("Use `services.AddLogging(lb => lb.AddXunitOutput([options => {}]))`.")]
public class XunitTestOutputLoggerProvider(ITestOutputHelperAccessor accessor, Func<string?, LogLevel, bool> filter)
    : XUnitLoggerProvider(new TestOutputHelperAccessorWrapper(accessor), new() { Filter = filter })
{
    /// <summary>Log minLevel LogLevel.Information</summary>
    public XunitTestOutputLoggerProvider(ITestOutputHelperAccessor accessor) : this(accessor,
        (_, level) => level is >= LogLevel.Information and < LogLevel.None) { }

    public static void Register(IServiceProvider provider) =>
        provider.GetRequiredService<ILoggerFactory>().AddProvider(new XUnitLoggerProvider(
            provider.GetService<TestOutputHelperAccessorWrapper>() ??
            new TestOutputHelperAccessorWrapper(provider.GetRequiredService<ITestOutputHelperAccessor>()),
            provider.GetRequiredService<IOptions<XUnitLoggerOptions>>().Value));

    public static void Register(IServiceProvider provider, LogLevel minimumLevel) =>
        provider.GetRequiredService<ILoggerFactory>().AddProvider(new XUnitLoggerProvider(
            provider.GetService<TestOutputHelperAccessorWrapper>() ??
            new TestOutputHelperAccessorWrapper(provider.GetRequiredService<ITestOutputHelperAccessor>()),
            new() { Filter = (_, level) => level >= minimumLevel && level < LogLevel.None }));
}
