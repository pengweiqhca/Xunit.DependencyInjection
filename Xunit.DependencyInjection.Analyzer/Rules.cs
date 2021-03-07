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
            CreateLocalizableString(nameof(Resources.MultipleConstructorTitle)),
           CreateLocalizableString(nameof(Resources.MultipleConstructorMessageFormat)),
            "Ctor", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.MultipleConstructorDescription)));

        public static DiagnosticDescriptor ParameterlessConstructor { get; } = new("XD002",
            CreateLocalizableString(nameof(Resources.ParameterlessConstructorTitle)),
           CreateLocalizableString(nameof(Resources.ParameterlessConstructorMessageFormat)),
            "Ctor", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.ParameterlessConstructorDescription)));

        public static DiagnosticDescriptor MultipleOverloads { get; } = new("XD003",
            CreateLocalizableString(nameof(Resources.MultipleOverloadsTitle)),
           CreateLocalizableString(nameof(Resources.MultipleOverloadsMessageFormat)),
            "Method", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.MultipleOverloadsDescription)));

        public static DiagnosticDescriptor NotStaticMethod { get; } = new("XD004",
            CreateLocalizableString(nameof(Resources.NotStaticMethodTitle)),
           CreateLocalizableString(nameof(Resources.NotStaticMethodMessageFormat)),
            "Method", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.NotStaticMethodDescription)));

        public static DiagnosticDescriptor NoReturnType { get; } = new("XD005",
            CreateLocalizableString(nameof(Resources.NoReturnTypeTitle)),
           CreateLocalizableString(nameof(Resources.NoReturnTypeMessageFormat)),
            "ReturnType", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.NoReturnTypeDescription)));

        public static DiagnosticDescriptor ReturnTypeAssignableTo { get; } = new("XD006",
            CreateLocalizableString(nameof(Resources.ReturnTypeAssignableToTitle)),
           CreateLocalizableString(nameof(Resources.ReturnTypeAssignableToMessageFormat)),
            "ReturnType", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.ReturnTypeAssignableToDescription)));

        public static DiagnosticDescriptor ParameterlessOrSingleParameter { get; } = new("XD007",
            CreateLocalizableString(nameof(Resources.ParameterlessOrSingleParameterTitle)),
           CreateLocalizableString(nameof(Resources.ParameterlessOrSingleParameterMessageFormat)),
            "ReturnType", DiagnosticSeverity.Error, true,
           CreateLocalizableString(nameof(Resources.ParameterlessOrSingleParameterDescription)));

        private static LocalizableString CreateLocalizableString(string key) => new LocalizableResourceString(key, Resources.ResourceManager, typeof(Resources));

        public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.CreateRange(typeof(Rules)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(DiagnosticDescriptor))
            .Select(p => (DiagnosticDescriptor)p.GetGetMethod().Invoke(null, Array.Empty<object>())));
    }
}
