using System;

namespace Xunit.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class StartupAttribute : Attribute
    {
        public StartupAttribute(Type startupType) => StartupType = startupType;

        public Type StartupType { get; }
    }
}
