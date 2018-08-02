using Xunit.Abstractions;

namespace Xunit.DependencyInjection
{
    public interface ITestOutputHelperAccessor
    {
        ITestOutputHelper Output { get; set; }
    }

    public class TestOutputHelperAccessor : ITestOutputHelperAccessor
    {
#if NET45
        private readonly string _name = System.Guid.NewGuid().ToString();

        public ITestOutputHelper Output
        {
            get => System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(_name) as ITestOutputHelper;
            set => System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(_name, value);
        }
#else
        private readonly System.Threading.AsyncLocal<ITestOutputHelper> _output = new System.Threading.AsyncLocal<ITestOutputHelper>();

        public ITestOutputHelper Output
        {
            get => _output.Value;
            set => _output.Value = value;
        }
#endif
    }
}
