﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Xunit.DependencyInjection.Analyzer.Test.Verifiers;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic();

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(descriptor);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
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

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
    public static Task VerifyCodeFixAsync(string source, string fixedSource, CancellationToken cancellationToken, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages([
                new("Microsoft.Extensions.Hosting", "8.0.0"),
                new("Xunit.DependencyInjection", "9.3.0")
            ]),
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(cancellationToken);
    }
}
