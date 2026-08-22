using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.DependencyInjection.Test;

public class TestClassByOrderOrderer : ITestClassOrderer
{
    public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
        where TTestClass : ITestClass => testClasses.OrderBy(tc => tc == null
            ? int.MaxValue
            : Type.GetType(tc.TestClassName)?.GetCustomAttribute<TestClassOrderAttribute>()?.Order ?? int.MaxValue)
        .CastOrToReadOnlyCollection();
}

[AttributeUsage(AttributeTargets.Class)]
public class TestClassOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
