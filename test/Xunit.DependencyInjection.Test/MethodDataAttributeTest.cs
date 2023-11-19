namespace Xunit.DependencyInjection.Test;

public class MethodDataAttributeTest
{
    private static readonly Guid RandomValue = Guid.NewGuid();

    [Theory]
    [MethodData(nameof(TestClassA.InstanceMethod), typeof(TestClassA))]
    public void InstanceMethodTest(Guid value) => Assert.Equal(RandomValue, value);

    [Theory]
    [MethodData(nameof(TestClassA.StaticMethod), typeof(TestClassA), null, 3)]
    public void StaticMethodTest(Guid value) => Assert.Equal(RandomValue, value);

    private class TestClassA
    {
        private readonly IDependency _dependency;

        public TestClassA(IDependency dependency)
        {
            Assert.NotNull(dependency);

            _dependency = dependency;
        }

        public IEnumerable<object?[]> InstanceMethod(IDependency dependency)
        {
            Assert.NotNull(dependency);

            Assert.Equal(_dependency, dependency);

            yield return [RandomValue];
        }

        public static IEnumerable<object?[]> StaticMethod([FromServices]IDependency dependency, int value)
        {
            Assert.NotNull(dependency);

            Assert.Equal(3, value);

            yield return [RandomValue];
        }
    }
}
