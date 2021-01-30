using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Xunit.DependencyInjection.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class XunitDependencyInjectionAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules.SupportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            switch (context.Symbol)
            {
                case IMethodSymbol m:
                    AnalyzeMethod(context, m);
                    break;
                case INamedTypeSymbol t:
                    AnalyzeType(context, t);
                    break;
            }
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, IMethodSymbol method)
        {
            if (!IsStartup(method.ContainingType) ||
                method.DeclaredAccessibility != Accessibility.Public) return;

            if (method.MethodKind == MethodKind.Constructor)
            {
                if (method.Parameters.Length > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.ParameterlessConstructor, method.Locations[0], method.Name));
                }
            }
            else if (method.MethodKind == MethodKind.DeclareMethod)
            {
                if (method.Name == "") { }
            }
        }

        private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol type)
        {
            if (!IsStartup(type)) return;

            if (type.Constructors.Count(ctor => ctor.DeclaredAccessibility == Accessibility.Public) > 1)
                context.ReportDiagnostic(Diagnostic.Create(Rules.MultipleConstructor, type.Locations[0], type.Name));
        }

        private static bool IsStartup(INamedTypeSymbol type)
        {
            //TODO Should get by other methods, default hard code.
            return type.Name == "Startup" && type.ContainingNamespace.Name == type.ContainingAssembly.Name;
        }

        //private static MethodInfo? FindMethod(Type startupType, string methodName) =>
        //    FindMethod(startupType, methodName, typeof(void));

        //private static MethodInfo? FindMethod(IMethodSymbol method, IReadOnlyList<Type> parameters, Type returnType)
        //{
        //    if (method.Name)
        //    {

        //        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
        //    }
        //}
    }
}
