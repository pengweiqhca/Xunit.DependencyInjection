namespace Xunit.DependencyInjection.Test;

public class RunMonitorCollectionLastOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections) =>
        testCollections.OrderBy(c => c.DisplayName.StartsWith("Monitor") || c.DisplayName.Contains(".Monitor")).ToArray();
}
