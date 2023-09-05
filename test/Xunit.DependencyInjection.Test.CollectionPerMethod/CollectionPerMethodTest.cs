namespace Xunit.DependencyInjection.Test.CollectionPerMethod;

public class CollectionPerMethodTest
{
    private readonly IDependency _dependency;

    public CollectionPerMethodTest(IDependency dependency)
        => _dependency = dependency;

    [Fact]
    public void FactTest()
    {
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void TheoryTest(int data)
    {

    }
}
