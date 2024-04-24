using System.Reflection;

namespace Xunit.DependencyInjection.Test;

public class BeforeAfterTestTest : IDisposable
{
    public IDependency? Dependency { get; set; }

    [Fact]
    public void Test() => Assert.NotNull(Dependency);

    public void Dispose() => Assert.Null(Dependency);
}

public class TestBeforeAfterTest(IDependency dependency) : BeforeAfterTest
{
    public override void Before(object? testClassInstance, MethodInfo methodUnderTest)
    {
        if (testClassInstance is BeforeAfterTestTest beforeAfterTestTest)
            beforeAfterTestTest.Dependency = dependency;
    }

    public override void After(object? testClassInstance, MethodInfo methodUnderTest)
    {
        if (testClassInstance is BeforeAfterTestTest beforeAfterTestTest)
            beforeAfterTestTest.Dependency = null;
    }
}
