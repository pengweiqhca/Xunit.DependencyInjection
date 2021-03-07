using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

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

            context.RegisterCompilationStartAction(SymbolAnalyzer.RegisterCompilationStartAction);
        }

        private class SymbolAnalyzer
        {
            private readonly INamedTypeSymbol _hostBuilder;
            private readonly INamedTypeSymbol _assemblyName;
            private readonly INamedTypeSymbol _serviceCollection;
            private readonly INamedTypeSymbol _hostBuilderContext;

            private SymbolAnalyzer(Compilation compilation)
            {
                var ass = compilation.References
                    .Select(compilation.GetAssemblyOrModuleSymbol)
                    .OfType<IAssemblySymbol>()
                    .ToArray();

                INamedTypeSymbol GetTypeSymbol(string name) => ass.Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(name)).First(t => t != null)!;

                _hostBuilder = GetTypeSymbol("Microsoft.Extensions.Hosting.IHostBuilder");
                _assemblyName = GetTypeSymbol(typeof(AssemblyName).FullName);
                _serviceCollection = GetTypeSymbol("Microsoft.Extensions.DependencyInjection.IServiceCollection");
                _hostBuilderContext = GetTypeSymbol("Microsoft.Extensions.Hosting.HostBuilderContext");
            }

            public static void RegisterCompilationStartAction(CompilationStartAnalysisContext csac)
            {
                // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
                csac.RegisterSymbolAction(new SymbolAnalyzer(csac.Compilation).AnalyzeSymbol, SymbolKind.Method, SymbolKind.NamedType);

            }

            private void AnalyzeSymbol(SymbolAnalysisContext context)
            {
                switch (context.Symbol)
                {
                    case INamedTypeSymbol type:
                        if (!IsStartup(type)) return;

                        if (type.InstanceConstructors.Count(ctor => ctor.DeclaredAccessibility == Accessibility.Public) > 1)
                            context.ReportDiagnostic(Diagnostic.Create(Rules.MultipleConstructor, type.Locations[0], type.Name));

                        AnalyzeOverride(context, type, "CreateHostBuilder");
                        AnalyzeOverride(context, type, "ConfigureHost");
                        AnalyzeOverride(context, type, "ConfigureServices");
                        AnalyzeOverride(context, type, "Configure");

                        return;
                    case IMethodSymbol method:
                        if (!IsStartup(method.ContainingType) || method.DeclaredAccessibility != Accessibility.Public) return;

                        if (method.IsStatic)
                            context.ReportDiagnostic(Diagnostic.Create(Rules.NotStaticMethod, method.Locations[0], method.Name));

                        switch (method.MethodKind)
                        {
                            case MethodKind.Constructor:
                                AnalyzeCtor(context, method);

                                return;
                            case MethodKind.Ordinary:
                                if ("CreateHostBuilder".Equals(method.Name, StringComparison.OrdinalIgnoreCase))
                                    AnalyzeCreateHostBuilder(context, method);
                                else if ("ConfigureHost".Equals(method.Name, StringComparison.OrdinalIgnoreCase))
                                    AnalyzeConfigureHost(context, method);
                                else if ("ConfigureServices".Equals(method.Name, StringComparison.OrdinalIgnoreCase))
                                    AnalyzeConfigureServices(context, method);
                                else if ("Configure".Equals(method.Name, StringComparison.OrdinalIgnoreCase))
                                    AnalyzeConfigure(context, method);

                                return;
                        }

                        return;
                }
            }

            private static bool IsStartup(ISymbol type)
            {
                //TODO Should get by other methods, default hard code.
                return type.Name == "Startup" && type.ContainingNamespace.Name == type.ContainingAssembly.Name;
            }

            private static void AnalyzeOverride(SymbolAnalysisContext context, ITypeSymbol type, string methodName)
            {
                var methods = type.GetMembers().OfType<IMethodSymbol>()
                    .Where(m => m.MethodKind == MethodKind.Ordinary &&
                                m.DeclaredAccessibility == Accessibility.Public &&
                                methodName.Equals(m.Name, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (methods.Length > 1)
                    context.ReportDiagnostic(Diagnostic.Create(Rules.MultipleOverloads, methods[0].Locations[0], methods[0].Name));
            }

            private static void AnalyzeReturnType(SymbolAnalysisContext context, IMethodSymbol method, ITypeSymbol? returnType)
            {
                if (returnType == null)
                {
                    if (!method.ReturnsVoid)
                        context.ReportDiagnostic(Diagnostic.Create(Rules.NoReturnType, method.Locations[0], method.Name));
                }
                else if (!IsAssignableFrom(returnType, method.ReturnType))
                    context.ReportDiagnostic(Diagnostic.Create(Rules.ReturnTypeAssignableTo, method.Locations[0], method.Name, returnType));
            }

            private static bool IsAssignableFrom(ITypeSymbol returnType, ITypeSymbol type)
            {
                if (SymbolEqualityComparer.Default.Equals(returnType, type)) return true;

                if (returnType.TypeKind == TypeKind.Interface)
                    return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(returnType, i));

                do
                {
                    if (SymbolEqualityComparer.Default.Equals(type, returnType)) return true;
                } while ((type = type.BaseType!) != null);

                return false;
            }

            private static void AnalyzeCtor(SymbolAnalysisContext context, IMethodSymbol method)
            {
                if (method.Parameters.Length > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.ParameterlessConstructor, method.Locations[0], method.Name));
                }
            }

            private void AnalyzeCreateHostBuilder(SymbolAnalysisContext context, IMethodSymbol method)
            {
                AnalyzeReturnType(context, method, _hostBuilder);

                if (method.Parameters.Length == 0) return;

                if (method.Parameters.Length > 1 || !SymbolEqualityComparer.Default.Equals(_assemblyName, method.Parameters[0].Type))
                    context.ReportDiagnostic(Diagnostic.Create(Rules.ParameterlessOrSingleParameter, method.Locations[0], method.Name, _hostBuilder.Name));
            }

            private static void AnalyzeConfigureHost(SymbolAnalysisContext context, IMethodSymbol method)
            {
                AnalyzeReturnType(context, method, null);
                //var parameters = method.GetParameters();
                //if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IHostBuilder))
                //    throw new InvalidOperationException($"The '{method.Name}' method of startup type '{startup.GetType().FullName}' must have the single 'IHostBuilder' parameter.");
            }

            private static void AnalyzeConfigureServices(SymbolAnalysisContext context, IMethodSymbol method)
            {
                AnalyzeReturnType(context, method, null);

                //var parameters = method.GetParameters();
                //builder.ConfigureServices(parameters.Length switch
                //{
                //    1 when parameters[0].ParameterType == typeof(IServiceCollection) =>
                //        (context, services) => method.Invoke(startup, new object[] { services }),
                //    2 when parameters[0].ParameterType == typeof(IServiceCollection) &&
                //           parameters[1].ParameterType == typeof(HostBuilderContext) =>
                //        (context, services) => method.Invoke(startup, new object[] { services, context }),
                //    2 when parameters[1].ParameterType == typeof(IServiceCollection) &&
                //           parameters[0].ParameterType == typeof(HostBuilderContext) =>
                //        (context, services) => method.Invoke(startup, new object[] { context, services }),
                //    _ => throw new InvalidOperationException($"The '{method.Name}' method in the type '{startup.GetType().FullName}' must have a 'IServiceCollection' parameter and optional 'HostBuilderContext' parameter.")
                //});
            }

            private static void AnalyzeConfigure(SymbolAnalysisContext context, IMethodSymbol method)
            {
                AnalyzeReturnType(context, method, null);
            }
        }
    }
}
