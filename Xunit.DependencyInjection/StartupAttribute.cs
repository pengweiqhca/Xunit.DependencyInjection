using System;

namespace Xunit.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class StartupAttribute : Attribute
    {
        public StartupAttribute(Type startupType) => StartupType = startupType;

        public Type StartupType { get; }

        /// <summary>Default is true. If false, a isolated Startup will be created for the test class.</summary>
        public bool Shared { get; set; } = true;
    }
}
