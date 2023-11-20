namespace Xunit.DependencyInjection.Test;

public class ParameterlessConstructorTest
{
    [Theory]
    [InlineData(null)]
    public void ITestOutputHelperAccessor_Output_Should_Not_Null([FromServices] ITestOutputHelperAccessor accessor) =>
        Assert.NotNull(accessor.Output);
}
