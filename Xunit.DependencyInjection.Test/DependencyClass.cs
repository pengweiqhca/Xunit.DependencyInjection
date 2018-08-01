using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.DependencyInjection.Test
{
    public interface IDependency
    {
        int Value { get; set; }

        int TestWriteLine();
    }

    internal class DependencyClass : IDependency
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DependencyClass(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public int Value { get; set; }

        public int TestWriteLine()
        {
            _testOutputHelper.WriteLine("test");
            return 1;
        }
    }
}
