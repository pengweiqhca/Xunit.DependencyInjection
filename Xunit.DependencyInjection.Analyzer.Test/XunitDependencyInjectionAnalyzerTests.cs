using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Analyzer.Test
{
    public class XunitDependencyInjectionAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(ReadFile))]
        public async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new Test<XunitDependencyInjectionAnalyzerAnalyzer>
            {
                TestCode = File.ReadAllText(Path.Combine("Startup", source)),
            };

            test.ExpectedDiagnostics.AddRange(expected);

            await test.RunAsync();
        }

        public static IEnumerable<object[]> ReadFile()
        {
            yield return new object[]
            {
                "CtorError.cs", new[]
                {
                    new DiagnosticResult(Rules.ParameterlessConstructor).WithSpan(5, 16, 5, 23).WithArguments(".ctor")
                }
            };
            yield return new object[] { "Empty.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[] { "MultipleCtor.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[]
            {
                "MultiplePublicCtor.cs", new[]
                {
                    new DiagnosticResult(Rules.MultipleConstructor).WithSpan(3, 18, 3, 25).WithArguments("Startup"),
                    new DiagnosticResult(Rules.ParameterlessConstructor).WithSpan(6, 16, 6, 23).WithArguments(".ctor")
                }
            };
            yield return new object[] { "NonStartup.cs", Array.Empty<DiagnosticResult>() };
        }

        private class Test<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
            where TAnalyzer : DiagnosticAnalyzer, new()
        {
            public Test()
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId)!.CompilationOptions!;
                    compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    return solution;
                });
            }
        }
    }
}
