namespace Xunit.DependencyInjection.Test;

public class ParameterlessConstructorTest([FromKeyedServices("Test")]IDependency keyedDependency)
{
    [Theory]
    [InlineData(null)]
    public void ITestOutputHelperAccessor_Output_Should_Not_Null([FromServices] ITestOutputHelperAccessor accessor)
    {
        keyedDependency.Value++;
        Assert.InRange(keyedDependency.Value, 1, 2);

        Assert.NotNull(accessor.Output);
    }
}
