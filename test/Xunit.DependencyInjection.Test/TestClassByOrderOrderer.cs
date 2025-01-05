using System.Reflection;

namespace Xunit.DependencyInjection.Test;

public class TestClassByOrderOrderer : ITestClassOrderer
{
    public IEnumerable<ITestClass> OrderTestClasses(IEnumerable<ITestClass> testCases) => testCases.OrderBy(tc =>
        Type.GetType(tc.TestClassName)?.GetCustomAttribute<TestClassOrderAttribute>()?.Order ?? int.MaxValue);
}

[AttributeUsage(AttributeTargets.Class)]
public class TestClassOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
