namespace Xunit.DependencyInjection;

public interface ITestOutputHelperAccessor
{
    ITestOutputHelper? Output { get; set; }
}

public class TestOutputHelperAccessor : ITestOutputHelperAccessor
{
    private readonly AsyncLocal<ITestOutputHelper?> _output = new();

    public ITestOutputHelper? Output
    {
        get => _output.Value;
        set => _output.Value = value;
    }
}
