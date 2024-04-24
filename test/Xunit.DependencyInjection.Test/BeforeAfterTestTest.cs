using System.Reflection;

namespace Xunit.DependencyInjection.Test;

public class BeforeAfterTestTest
{
    public IDependency? Dependency { get; set; }

    [Fact]
    public void Test() => Assert.NotNull(Dependency);
}

public class TestBeforeAfterTest(IDependency dependency) : BeforeAfterTest
{
    public override void Before(object? testClassInstance, MethodInfo methodUnderTest)
    {
        if (testClassInstance is BeforeAfterTestTest beforeAfterTestTest)
            beforeAfterTestTest.Dependency = dependency;
    }
}
