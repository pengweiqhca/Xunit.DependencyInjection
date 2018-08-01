using System.Threading;
using Xunit.Abstractions;

namespace Xunit.DependencyInjection
{
    public interface ITestOutputHelperAccessor
    {
        ITestOutputHelper Output { get; set; }
    }

    public class TestOutputHelperAccessor : ITestOutputHelperAccessor
    {
        private AsyncLocal<ITestOutputHelper> _output = new AsyncLocal<ITestOutputHelper>();

        public ITestOutputHelper Output
        {
            get => _output.Value;
            set => _output.Value = value;
        }
    }
}
