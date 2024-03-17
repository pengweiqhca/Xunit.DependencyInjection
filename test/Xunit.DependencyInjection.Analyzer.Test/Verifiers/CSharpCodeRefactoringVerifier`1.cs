using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Immutable;

namespace Xunit.DependencyInjection.Analyzer.Test.Verifiers;

public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult[], string)"/>
    public static Task VerifyRefactoringAsync(string source, CancellationToken cancellationToken, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages([
                new("Microsoft.Extensions.Hosting", "2.1.0"),
                new("Xunit.DependencyInjection", "7.1.0")
            ]),
        };

        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync(cancellationToken);
    }
}
