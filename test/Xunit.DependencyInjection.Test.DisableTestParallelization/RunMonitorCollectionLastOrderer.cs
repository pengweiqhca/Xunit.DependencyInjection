using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.DependencyInjection.Test.Parallelization;

public class RunMonitorCollectionLastOrderer : ITestCollectionOrderer
{
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections) where TTestCollection : ITestCollection =>
        testCollections.OrderBy(c => c.TestCollectionDisplayName.EndsWith("MonitorTest")).ToArray();

}
