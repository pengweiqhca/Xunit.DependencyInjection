using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;

namespace Xunit.DependencyInjection.Analyzer;

public static class Rules
{
    public static DiagnosticDescriptor MultipleConstructor { get; } = new("XD001",
        CreateLocalizableString(nameof(AnalyzerResources.MultipleConstructorTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.MultipleConstructorMessageFormat)),
        "Startup constructor", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.MultipleConstructorDescription)));

    public static DiagnosticDescriptor ParameterlessConstructor { get; } = new("XD002",
        CreateLocalizableString(nameof(AnalyzerResources.ParameterlessConstructorTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.ParameterlessConstructorMessageFormat)),
        "Startup constructor", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.ParameterlessConstructorDescription)));

    public static DiagnosticDescriptor MultipleOverloads { get; } = new("XD003",
        CreateLocalizableString(nameof(AnalyzerResources.MultipleOverloadsTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.MultipleOverloadsMessageFormat)),
        "Startup method", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.MultipleOverloadsDescription)));

    public static DiagnosticDescriptor NoReturnType { get; } = new("XD005",
        CreateLocalizableString(nameof(AnalyzerResources.NoReturnTypeTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.NoReturnTypeMessageFormat)),
        "Startup method returnType", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.NoReturnTypeDescription)));

    public static DiagnosticDescriptor ReturnTypeAssignableTo { get; } = new("XD006",
        CreateLocalizableString(nameof(AnalyzerResources.ReturnTypeAssignableToTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.ReturnTypeAssignableToMessageFormat)),
        "Startup method returnType", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.ReturnTypeAssignableToDescription)));

    public static DiagnosticDescriptor ParameterlessOrSingleParameter { get; } = new("XD007",
        CreateLocalizableString(nameof(AnalyzerResources.ParameterlessOrSingleParameterTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.ParameterlessOrSingleParameterMessageFormat)),
        "Startup method parameter", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.ParameterlessOrSingleParameterDescription)));

    public static DiagnosticDescriptor SingleParameter { get; } = new("XD008",
        CreateLocalizableString(nameof(AnalyzerResources.SingleParameterTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.SingleParameterMessageFormat)),
        "Startup method parameter", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.SingleParameterDescription)));

    public static DiagnosticDescriptor ConfigureServices { get; } = new("XD009",
        CreateLocalizableString(nameof(AnalyzerResources.ConfigureServicesTitle)),
        CreateLocalizableString(nameof(AnalyzerResources.ConfigureServicesMessageFormat)),
        "Startup method parameter", DiagnosticSeverity.Error, true,
        CreateLocalizableString(nameof(AnalyzerResources.ConfigureServicesDescription)));

    private static LocalizableResourceString CreateLocalizableString(string key) =>
        new(key, AnalyzerResources.ResourceManager, typeof(AnalyzerResources));

    public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.CreateRange(typeof(Rules)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.PropertyType == typeof(DiagnosticDescriptor))
        .Select(p => p.GetMethod)
        .Where(m => m != null)
        .Select(m => (DiagnosticDescriptor)m!.Invoke(null, [])!));
}
