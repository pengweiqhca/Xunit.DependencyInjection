using Xunit;
using Xunit.v3;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Xunit.DependencyInjection.Test;

public class RunMonitorCollectionLastOrderer : ITestCollectionOrderer
{
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections) where TTestCollection : ITestCollection =>
        testCollections.OrderBy(c => c.TestCollectionDisplayName.StartsWith("Monitor")).ToArray();
}
