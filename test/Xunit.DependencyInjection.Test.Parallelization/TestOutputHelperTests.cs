using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = false)]

namespace Xunit.DependencyInjection.Test.Parallelization;

public class TestOutputHelperTests
{
    private readonly ITestOutputHelper _output;

    public TestOutputHelperTests(ITestOutputHelper output) => this._output = output;

    [Fact]
    public void Fact1() => _output.WriteLine(nameof(Fact1));

    [Fact]
    public void Fact2() => _output.WriteLine(nameof(Fact2));

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Theory(int value) => _output.WriteLine(nameof(Theory) + value);
}
