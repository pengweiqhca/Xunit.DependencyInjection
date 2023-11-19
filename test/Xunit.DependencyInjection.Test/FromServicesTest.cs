namespace Xunit.DependencyInjection.Test;

public class FromServicesTest(IDependency dependency, ITestOutputHelper output)
{
    [Theory]
    [InlineData("Test", 3, null, null)]
    public void FactTest(string arg1,
        [FromServices]int invalid,
        [FromServices]IDependency dependency1,
        [FromServices]ITestOutputHelper output1)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(3, invalid);
        Assert.Equal(dependency, dependency1);
        Assert.Equal(output, output1);
    }

    [Theory]
    [MemberData(nameof(TheoryData))]
    public void TheoryTest(string arg1,
        [FromServices]int invalid,
        [FromServices]IDependency dependency1,
        [FromServices]ITestOutputHelper output1)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(3, invalid);
        Assert.Equal(dependency, dependency1);
        Assert.Equal(output, output1);
    }

    public static IEnumerable<object?[]> TheoryData()
    {
        yield return ["Test", 3, null, null];
    }
}
