namespace Xunit.DependencyInjection;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class StartupTypeAttribute : Attribute
{
    public string TypeName { get; }
    public string? AssemblyName { get; }

    /// <summary>
    /// Initializes an instance of <see cref="StartupTypeAttribute" />.
    /// </summary>
    /// <param name="typeName">The fully qualified type name of the startup
    /// (f.e., 'Xunit.DependencyInjection.Test.Startup')</param>
    /// <param name="assemblyName">The name of the assembly that the startup type
    /// is located in, without file extension (f.e., 'Xunit.DependencyInjection.Test')</param>
    public StartupTypeAttribute(string typeName, string? assemblyName = null)
    {
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        AssemblyName = assemblyName;
    }

    /// <summary>
    /// Initializes an instance of <see cref="StartupTypeAttribute" />.
    /// </summary>
    /// <param name="startupType">The fully qualified type name of the startup type
    /// (f.e., 'Xunit.DependencyInjection.Test.Startup')</param>
    public StartupTypeAttribute(Type startupType)
    {
        if (startupType?.FullName == null) throw new ArgumentNullException(nameof(startupType));

        TypeName = startupType.FullName;
        AssemblyName = startupType.Assembly.FullName;
    }
}
