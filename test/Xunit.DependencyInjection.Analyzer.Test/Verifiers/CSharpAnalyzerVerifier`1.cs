using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.DependencyInjection.Analyzer.Test.Verifiers;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(descriptor);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static Task VerifyAnalyzerAsync(string source, CancellationToken cancellationToken, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages([
                new("Microsoft.Extensions.Hosting", "8.0.0"),
                new("Xunit.DependencyInjection", "9.3.0")
            ]),
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(cancellationToken);
    }
}
