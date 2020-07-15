using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

[assembly: TestCollectionOrderer("Xunit.DependencyInjection.Test.RunMonitorCollectionLastOrderer", "Xunit.DependencyInjection.Test")]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Xunit.DependencyInjection.Test
{
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
}
