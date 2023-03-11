using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using CSharpAnalyzerVerifier = Xunit.DependencyInjection.Analyzer.Test.Verifiers.CSharpAnalyzerVerifier<
    Xunit.DependencyInjection.Analyzer.XunitDependencyInjectionAnalyzer>;
using CSharpCodeFixVerifier = Xunit.DependencyInjection.Analyzer.Test.Verifiers.CSharpCodeFixVerifier<
    Xunit.DependencyInjection.Analyzer.XunitDependencyInjectionAnalyzer,
    Xunit.DependencyInjection.Analyzer.XunitDependencyInjectionCodeFixProvider>;

namespace Xunit.DependencyInjection.Analyzer.Test;

public class DependencyInjectionTest
{
    private readonly CancellationToken _cancellationToken;

    public DependencyInjectionTest(CancellationToken cancellationToken) => _cancellationToken = cancellationToken;

    [Theory]
    [MemberData(nameof(ReadFile))]
    public async Task VerifyAsync(string source, string? fixedSource, params DiagnosticResult[] expected)
    {
        var task = fixedSource == null
            ? CSharpAnalyzerVerifier.VerifyAnalyzerAsync(
#if NETFRAMEWORK
                     File.ReadAllText(Path.Combine("Startup", source)),
#else
                await File.ReadAllTextAsync(Path.Combine("Startup", source), _cancellationToken).ConfigureAwait(false),
#endif
                _cancellationToken, expected)
            : CSharpCodeFixVerifier.VerifyCodeFixAsync(
#if NETFRAMEWORK
                     File.ReadAllText(Path.Combine("Startup", source)),
                     File.ReadAllText(Path.Combine("Startup", fixedSource)),
#else
                await File.ReadAllTextAsync(Path.Combine("Startup", source), _cancellationToken).ConfigureAwait(false),
                await File.ReadAllTextAsync(Path.Combine("Startup", fixedSource), _cancellationToken).ConfigureAwait(false),
#endif
                _cancellationToken, expected);

        await task.ConfigureAwait(false);
    }

    public static IEnumerable<object?[]> ReadFile()
    {
        yield return new object?[]
        {
            "ConfigureHostTestStartup0.cs", null, new[]
            {
                new DiagnosticResult(Rules.SingleParameter).WithSpan(5, 21, 5, 34).WithArguments("ConfigureHost", nameof(IHostBuilder))
            }
        };
        yield return new object?[]
        {
            "ConfigureHostTestStartup1.cs", null, new[]
            {
                new DiagnosticResult(Rules.SingleParameter).WithSpan(5, 21, 5, 34).WithArguments("ConfigureHost", nameof(IHostBuilder))
            }
        };
        yield return new object?[]
        {
            "ConfigureHostTestStartup2.cs", "ConfigureHostTestStartup3.cs", new[]
            {
                new DiagnosticResult(Rules.NoReturnType).WithSpan(8, 23, 8, 36).WithArguments("ConfigureHost")
            }
        };
        yield return new object?[] { "ConfigureHostTestStartup3.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[]
        {
            "ConfigureServicesTestStartup0.cs", null, new[]
            {
                new DiagnosticResult(Rules.NoReturnType).WithSpan(7, 23, 7, 40).WithArguments("ConfigureServices")
            }
        };
        yield return new object?[] { "ConfigureServicesTestStartup1.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[] { "ConfigureServicesTestStartup2.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[] { "ConfigureServicesTestStartup3.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[]
        {
            "ConfigureServicesTestStartup4.cs", null, new[]
            {
                new DiagnosticResult(Rules.ConfigureServices).WithSpan(7, 21, 7, 38).WithArguments("ConfigureServices")
            }
        };
        yield return new object?[]
        {
            "ConfigureServicesTestStartup5.cs", null, new[]
            {
                new DiagnosticResult(Rules.ConfigureServices).WithSpan(8, 21, 8, 38).WithArguments("ConfigureServices")
            }
        };
        yield return new object?[]
        {
            "ConfigureServicesTestStartup6.cs", null, new[]
            {
                new DiagnosticResult(Rules.ConfigureServices).WithSpan(5, 21, 5, 38).WithArguments("ConfigureServices")
            }
        };
        yield return new object?[] { "ConfigureTestStartup0.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[]
        {
            "ConfigureTestStartup1.cs", null, new[]
            {
                new DiagnosticResult(Rules.NoReturnType).WithSpan(5, 23, 5, 32).WithArguments("Configure")
            }
        };
        yield return new object?[]
        {
            "CreateHostBuilderTestStartup0.cs", "CreateHostBuilderTestStartup2.cs", new[]
            {
                new DiagnosticResult(Rules.ReturnTypeAssignableTo).WithSpan(7, 21, 7, 38).WithArguments("CreateHostBuilder", typeof(IHostBuilder).FullName!)
            }
        };
        yield return new object?[]
        {
            "CreateHostBuilderTestStartup1.cs", null, new[]
            {
                new DiagnosticResult(Rules.ReturnTypeAssignableTo).WithSpan(5, 23, 5, 40).WithArguments("CreateHostBuilder", typeof(IHostBuilder).FullName!)
            }
        };
        yield return new object?[] { "CreateHostBuilderTestStartup2.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[] { "CreateHostBuilderTestStartup3.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[]
        {
            "CreateHostBuilderTestStartup4.cs", null, new[]
            {
                new DiagnosticResult(Rules.ParameterlessOrSingleParameter).WithSpan(7, 29, 7, 46).WithArguments("CreateHostBuilder", nameof(AssemblyName))
            }
        };
        yield return new object?[]
        {
            "CtorError.cs", null, new[]
            {
                new DiagnosticResult(Rules.ParameterlessConstructor).WithSpan(9, 16, 9, 24).WithArguments(".ctor")
            }
        };
        yield return new object?[] { "Empty.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[] { "MultipleCtor.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[]
        {
            "MultiplePublicCtor.cs", null, new[]
            {
                new DiagnosticResult(Rules.MultipleConstructor).WithSpan(8, 16, 8, 24).WithArguments(".ctor"),
                new DiagnosticResult(Rules.MultipleConstructor).WithSpan(9, 16, 9, 24).WithArguments(".ctor"),
                new DiagnosticResult(Rules.ParameterlessConstructor).WithSpan(9, 16, 9, 24).WithArguments(".ctor")
            }
        };
        yield return new object?[] { "NonStartup.cs", null, Array.Empty<DiagnosticResult>() };
        yield return new object?[] { "StaticMethod.cs", null, Array.Empty<DiagnosticResult>() };
    }
}