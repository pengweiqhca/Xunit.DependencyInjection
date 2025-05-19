using Xunit.v3;

namespace Xunit.DependencyInjection.Test;

public class RunMonitorCollectionLastOrderer : ITestCollectionOrderer
{
    public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections) where TTestCollection : ITestCollection =>
        testCollections.OrderBy(c => c.TestCollectionDisplayName.StartsWith("Monitor") || c.TestCollectionDisplayName.Contains(".Monitor")).ToArray();
}
