using System;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Xunit.DependencyInjection.Analyzer
{
    public static class Rules
    {
        public static DiagnosticDescriptor MultipleConstructor { get; } = new("XD001",
            new LocalizableResourceString(nameof(Resources.MultipleConstructorTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.MultipleConstructorMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Ctor", DiagnosticSeverity.Error, true,
            new LocalizableResourceString(nameof(Resources.MultipleConstructorDescription), Resources.ResourceManager, typeof(Resources)));

        public static DiagnosticDescriptor ParameterlessConstructor { get; } = new("XD002",
            new LocalizableResourceString(nameof(Resources.ParameterlessConstructorTitle), Resources.ResourceManager, typeof(Resources)),
            new LocalizableResourceString(nameof(Resources.ParameterlessConstructorMessageFormat), Resources.ResourceManager, typeof(Resources)),
            "Ctor", DiagnosticSeverity.Error, true,
            new LocalizableResourceString(nameof(Resources.ParameterlessConstructorDescription), Resources.ResourceManager, typeof(Resources)));

        public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.CreateRange(typeof(Rules)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(DiagnosticDescriptor))
            .Select(p => (DiagnosticDescriptor)p.GetGetMethod().Invoke(null, Array.Empty<object>())));
    }
}
