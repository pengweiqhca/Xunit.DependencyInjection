namespace Xunit.DependencyInjection.Test;

public class FromKeyedServicesConstructorTest(
    [FromKeyedServices("Test")] IDependency keyedDependency,
    IDependency dependency)
{
    [Theory]
    [InlineData(null)]
    public void ITestOutputHelperAccessor_Output_Should_Not_Null([FromServices] ITestOutputHelperAccessor accessor)
    {
        Assert.NotEqual(keyedDependency, dependency);

        keyedDependency.Value++;
        Assert.InRange(keyedDependency.Value, 1, 2);

        Assert.NotNull(accessor.Output);
    }
}
