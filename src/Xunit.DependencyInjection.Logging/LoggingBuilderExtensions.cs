﻿using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xunit.DependencyInjection.Logging;

public static class LoggingBuilderExtensions
{
    extension(ILoggingBuilder builder)
    {
        public ILoggingBuilder AddXunitOutput()
        {
            builder.Services.TryAddSingleton<TestOutputHelperAccessorWrapper>();

            builder.Services.AddSingleton<ILoggerProvider>(provider => new XUnitLoggerProvider(
                provider.GetRequiredService<TestOutputHelperAccessorWrapper>(),
                provider.GetRequiredService<IOptions<XUnitLoggerOptions>>().Value));

            return builder;
        }

        public ILoggingBuilder AddXunitOutput(Action<XUnitLoggerOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder.AddXunitOutput();
        }
    }
}
