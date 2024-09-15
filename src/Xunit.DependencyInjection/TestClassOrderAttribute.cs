namespace Xunit.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public class TestClassOrderAttribute: Attribute
{
    public int Order { get; }

    public TestClassOrderAttribute(int order)
    {
        Order = order;
    }
}
