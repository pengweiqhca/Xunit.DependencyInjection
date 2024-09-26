using Xunit;
using Xunit.DependencyInjection.Test;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Xunit.DependencyInjection.Test;

public class RunMonitorCollectionLastOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections) =>
        testCollections.OrderBy(c => c.DisplayName.StartsWith("Monitor"));
}
