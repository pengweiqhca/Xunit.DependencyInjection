﻿namespace Xunit.DependencyInjection;

public class DependencyInjectionTestFrameworkExecutor(
    IXunitTestAssembly testAssembly,
    ParallelizationMode parallelizationMode)
    : XunitTestFrameworkExecutor(testAssembly)
{
    public override async ValueTask RunTestCases(IReadOnlyCollection<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        var exceptions = new List<Exception>();

        await using var hostManager = new HostManager(TestAssembly.Assembly, executionMessageSink);

        var context = GetHost(exceptions, hostManager.BuildDefaultHost);

        var contextMap = testCases
            .GroupBy(tc => tc.TestMethod.TestClass)
            .ToDictionary(group => group.Key,
                group => GetHost(exceptions, () => hostManager.GetContext(group.Key.Class)));

        try
        {
            await hostManager.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        var host = context == null || context.Disposed ? null : context.Host;

        await new DependencyInjectionTestAssemblyRunner(new(host, parallelizationMode, contextMap), exceptions)
            .Run(new(TestAssembly, host?.Services), testCases, executionMessageSink,
                executionOptions, cancellationToken);

        await hostManager.StopAsync(cancellationToken);
        return;

        static DependencyInjectionBuildContext? GetHost(ICollection<Exception> exceptions,
            Func<DependencyInjectionBuildContext?> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex.Unwrap());
            }

            return default;
        }
    }
}

public sealed class DependencyInjectionTestAssembly(
    IXunitTestAssembly testAssembly,
    IServiceProvider? defaultRootServices) : IXunitTestAssembly
{
    public string AssemblyName => testAssembly.AssemblyName;

    public string AssemblyPath => testAssembly.AssemblyPath;

    public string? ConfigFilePath => testAssembly.ConfigFilePath;

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits => testAssembly.Traits;

    public string UniqueID => testAssembly.UniqueID;

    public Guid ModuleVersionID => testAssembly.ModuleVersionID;

    public Assembly Assembly => testAssembly.Assembly;

    IReadOnlyCollection<Type> IXunitTestAssembly.AssemblyFixtureTypes { get; } = testAssembly.AssemblyFixtureTypes
        .Where(TestHelper.GenericTypeArgumentIsGenericParameter).ToArray();

    public IReadOnlyCollection<Type> AssemblyFixtureTypes { get; } = testAssembly.AssemblyFixtureTypes
        .WhereNot(TestHelper.GenericTypeArgumentIsGenericParameter).ToArray();

    public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
        testAssembly.BeforeAfterTestAttributes;

    public ICollectionBehaviorAttribute? CollectionBehavior => testAssembly.CollectionBehavior;

    public IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)>
        CollectionDefinitions => testAssembly.CollectionDefinitions;

    public string TargetFramework => testAssembly.TargetFramework;

    public ITestCaseOrderer? TestCaseOrderer =>
        defaultRootServices?.GetService<ITestCaseOrderer>() ?? testAssembly.TestCaseOrderer;

    public ITestCollectionOrderer? TestCollectionOrderer =>
        defaultRootServices?.GetService<ITestCollectionOrderer>() ?? testAssembly.TestCollectionOrderer;

    public Version Version => testAssembly.Version;
}
