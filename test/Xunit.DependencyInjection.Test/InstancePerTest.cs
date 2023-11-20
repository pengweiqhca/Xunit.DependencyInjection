namespace Xunit.DependencyInjection.Test;

public class InstancePerTest
{
    private readonly IDependency _d;

    public InstancePerTest(IDependency d, CancellationToken cancellationToken)
    {
        _d = d;

        Assert.NotEqual(CancellationToken.None, cancellationToken);
    }

    [Fact]
    public void Test1()
    {
        _d.Value++;

        Assert.Equal(1, _d.Value);
    }

    [Fact]
    public void Test2()
    {
        _d.Value++;

        Assert.Equal(1, _d.Value);
    }

    [Fact]
    public void Test3() => _d.TestWriteLine(100);
}
