namespace Xunit.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StartupAttribute(Type startupType) : Attribute
{
    public Type StartupType { get; } = startupType;

    /// <summary>Default is true. If false, a isolated Startup will be created for the test class.</summary>
    public bool Shared { get; set; } = true;
}