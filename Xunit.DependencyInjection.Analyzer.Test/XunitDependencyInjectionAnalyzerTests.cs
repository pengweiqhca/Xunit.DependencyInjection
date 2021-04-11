using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Analyzer.Test
{
    public class XunitDependencyInjectionAnalyzerTests
    {
        [Theory]
        [MemberData(nameof(ReadFile))]
        public async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new Test<XunitDependencyInjectionAnalyzer>
            {
#if NETFRAMEWORK
                TestCode = File.ReadAllText(Path.Combine("Startup", source)),
#else
                TestCode = await File.ReadAllTextAsync(Path.Combine("Startup", source)),
#endif
                ReferenceAssemblies = ReferenceAssemblies.Default.AddPackages(
                    ImmutableArray.Create(
                        new PackageIdentity("Microsoft.Extensions.DependencyInjection.Abstractions", "2.1.0"),
                        new PackageIdentity("Microsoft.Extensions.Hosting", "2.1.0"),
                        new PackageIdentity("Microsoft.Extensions.Hosting.Abstractions", "2.1.0")))
            };

            test.ExpectedDiagnostics.AddRange(expected);

            await test.RunAsync();
        }

        public static IEnumerable<object[]> ReadFile()
        {
            yield return new object[]
            {
                "ConfigureHostTestStartup0.cs", new[]
                {
                    new DiagnosticResult(Rules.SingleParameter).WithSpan(5, 21, 5, 34).WithArguments("ConfigureHost", nameof(IHostBuilder))
                }
            };
            yield return new object[]
            {
                "ConfigureHostTestStartup1.cs", new[]
                {
                    new DiagnosticResult(Rules.SingleParameter).WithSpan(5, 21, 5, 34).WithArguments("ConfigureHost", nameof(IHostBuilder))
                }
            };
            yield return new object[]
            {
                "ConfigureHostTestStartup2.cs", new[]
                {
                    new DiagnosticResult(Rules.NoReturnType).WithSpan(7, 23, 7, 36).WithArguments("ConfigureHost")
                }
            };
            yield return new object[] { "ConfigureHostTestStartup3.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[]
            {
                "ConfigureServicesTestStartup0.cs", new[]
                {
                    new DiagnosticResult(Rules.NoReturnType).WithSpan(7, 23, 7, 40).WithArguments("ConfigureServices")
                }
            };
            yield return new object[] { "ConfigureServicesTestStartup1.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[] { "ConfigureServicesTestStartup2.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[] { "ConfigureServicesTestStartup3.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[]
            {
                "ConfigureServicesTestStartup4.cs", new[]
                {
                    new DiagnosticResult(Rules.ConfigureServices).WithSpan(7, 21, 7, 38).WithArguments("ConfigureServices")
                }
            };
            yield return new object[]
            {
                "ConfigureServicesTestStartup5.cs", new[]
                {
                    new DiagnosticResult(Rules.ConfigureServices).WithSpan(8, 21, 8, 38).WithArguments("ConfigureServices")
                }
            };
            yield return new object[]
            {
                "ConfigureServicesTestStartup6.cs", new[]
                {
                    new DiagnosticResult(Rules.ConfigureServices).WithSpan(5, 21, 5, 38).WithArguments("ConfigureServices")
                }
            };
            yield return new object[] { "ConfigureTestStartup0.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[]
            {
                "ConfigureTestStartup1.cs", new[]
                {
                    new DiagnosticResult(Rules.NoReturnType).WithSpan(5, 23, 5, 32).WithArguments("Configure")
                }
            };
            yield return new object[]
            {
                "CreateHostBuilderTestStartup0.cs", new[]
                {
                    new DiagnosticResult(Rules.ReturnTypeAssignableTo).WithSpan(5, 21, 5, 38).WithArguments("CreateHostBuilder", typeof(IHostBuilder).FullName!)
                }
            };
            yield return new object[]
            {
                "CreateHostBuilderTestStartup1.cs", new[]
                {
                    new DiagnosticResult(Rules.ReturnTypeAssignableTo).WithSpan(5, 23, 5, 40).WithArguments("CreateHostBuilder", typeof(IHostBuilder).FullName!)
                }
            };
            yield return new object[] { "CreateHostBuilderTestStartup2.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[] { "CreateHostBuilderTestStartup3.cs", Array.Empty<DiagnosticResult>() };
            yield return new object[]
            {
                "CreateHostBuilderTestStartup4.cs", new[]
                {
                    new DiagnosticResult(Rules.ParameterlessOrSingleParameter).WithSpan(7, 29, 7, 46).WithArguments("CreateHostBuilder", nameof(AssemblyName))
                }
            };
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
