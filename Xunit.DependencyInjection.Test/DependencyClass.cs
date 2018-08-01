using System;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.DependencyInjection.Test
{
    public interface IDependency
    {
        int Value { get; set; }

        int TestWriteLine(int count);
    }

    internal class DependencyClass : IDependency
    {
        private readonly ITestOutputHelperAccessor _testOutputHelperAccessor;

        public DependencyClass(ITestOutputHelperAccessor testOutputHelperAccessor)
        {
            _testOutputHelperAccessor = testOutputHelperAccessor;
        }

        public int Value { get; set; }

        public int TestWriteLine(int count)
        {
            var output = _testOutputHelperAccessor.Output;
            if (output != null)
                for (var index = 0; index < count; index++)
                {
                    output.WriteLine($"{DateTime.Now:ss.fff} test {index}");
                    Thread.Sleep(1);
                }

            return 1;
        }
    }
}
