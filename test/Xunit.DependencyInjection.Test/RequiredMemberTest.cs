using System.Diagnostics.CodeAnalysis;

namespace Xunit.DependencyInjection.Test;

public class RequiredMemberTest
{
    public required ITestOutputHelper Output { get; set; }

    public required IDependency Dependency { get; init; }

    [Fact]
    public void Test()
    {
        Assert.NotNull(Output);
        Assert.NotNull(Dependency);
    }
}

public class SetsRequiredMembersTest
{
    private readonly bool _isCtorSet;
    private ITestOutputHelper _output;
    private readonly IDependency _dependency;

    [method: SetsRequiredMembers]
    public SetsRequiredMembersTest(ITestOutputHelper output, IDependency dependency)
    {
        _output = Output = output;
        _dependency = Dependency = dependency;

        _isCtorSet = true;
    }

    public required ITestOutputHelper Output
    {
        get => _output;
        set
        {
            if (_output != null) throw new InvalidOperationException();

            _output = value;
        }
    }

    public required IDependency Dependency
    {
        get => _dependency;
        init
        {
            if (_dependency != null) throw new InvalidOperationException();

            _dependency = value;
        }
    }

    [Fact]
    public void Test()
    {
        Assert.True(_isCtorSet);
        Assert.NotNull(Output);
        Assert.NotNull(Dependency);
    }
}
