using Xunit;
using Xunit.DependencyInjection.Test;

[assembly: TestCollectionOrderer("Xunit.DependencyInjection.Test." + nameof(RunMonitorCollectionLastOrderer), "Xunit.DependencyInjection.Test")]
[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Xunit.DependencyInjection.Test;

public class RunMonitorCollectionLastOrderer : ITestCollectionOrderer
{
    public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
    {
        var monitorCollections = testCollections.Where(c => c.DisplayName.StartsWith("Monitor"));

        var result = testCollections
            .Where(c => c.DisplayName.StartsWith("Monitor") == false)
            .Concat(monitorCollections);

        return result;
    }
}
