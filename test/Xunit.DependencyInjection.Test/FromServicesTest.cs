namespace Xunit.DependencyInjection.Test;

public class FromServicesTest
{
    private readonly IDependency _dependency;
    private readonly ITestOutputHelper _output;

    public FromServicesTest(IDependency dependency, ITestOutputHelper output)
    {
        _dependency = dependency;
        _output = output;
    }

    [Theory]
    [InlineData("Test", 3, null, null)]
    public void FactTest(string arg1,
        [FromServices]int invalid,
        [FromServices]IDependency dependency,
        [FromServices]ITestOutputHelper output)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(3, invalid);
        Assert.Equal(_dependency, dependency);
        Assert.Equal(_output, output);
    }

    [Theory]
    [MemberData(nameof(TheoryData))]
    public void TheoryTest(string arg1,
        [FromServices]int invalid,
        [FromServices]IDependency dependency,
        [FromServices]ITestOutputHelper output)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(3, invalid);
        Assert.Equal(_dependency, dependency);
        Assert.Equal(_output, output);
    }

    public static IEnumerable<object?[]> TheoryData()
    {
        yield return new object?[] { "Test", 3, null, null };
    }
}
