namespace Xunit.DependencyInjection.Test;

public class TestClassByOrderOrderer : ITestClassOrderer
{
    public IEnumerable<ITestClass> OrderTestClasses(IEnumerable<ITestClass> testCases) => testCases.OrderBy(tc =>
    {
        var classOrderAttribute = tc.Class.GetCustomAttributes(typeof(TestClassOrderAttribute)).FirstOrDefault();

        return classOrderAttribute?.GetNamedArgument<int>(nameof(TestClassOrderAttribute.Order)) ?? int.MaxValue;
    });
}

[AttributeUsage(AttributeTargets.Class)]
public class TestClassOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
