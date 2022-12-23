using xRetry;

namespace Xunit.DependencyInjection.Test;

public class XRetryTest
{
    private readonly IDependency _dependency;
    private static int _factNumCalls = 0;
    // testId => numCalls
    private static readonly Dictionary<int, int> _theoryNumCalls = new()
    {
        { 0, 0 },
        { 1, 0 }
    };
    // testId => numCalls
    private static readonly Dictionary<int, int> _nonSerializableTheoryNumCalls = new()
    {
        { 0, 0 },
        { 1, 0 }
    };

    public XRetryTest(IDependency dependency)
    {
        _dependency = dependency;
    }

    [RetryFact]
    public void RetryFact()
    {
        AssertDependencyUnique();

        _factNumCalls++;
        Assert.Equal(3, _factNumCalls);
    }

    [RetryTheory]
    [InlineData(0)]
    [InlineData(1)]
    public void RetryTheory(int id)
    {
        AssertDependencyUnique();

        _theoryNumCalls[id]++;
        Assert.Equal(3, _theoryNumCalls[id]);
    }

    public class NonSerializableTestData
    {
        public readonly int Id;

        public NonSerializableTestData(int id)
        {
            Id = id;
        }
    }

    [RetryTheory]
    [MemberData(nameof(GetTestData))]
    public void RetryTheoryNonSerializableData(NonSerializableTestData nonSerializableWrapper)
    {
        AssertDependencyUnique();

        _nonSerializableTheoryNumCalls[nonSerializableWrapper.Id]++;
        Assert.Equal(3, _nonSerializableTheoryNumCalls[nonSerializableWrapper.Id]);
    }

    public static IEnumerable<object[]> GetTestData() => new[]
    {
        new object[] { new NonSerializableTestData(0) },
        new object[] { new NonSerializableTestData(1) }
    };

    private void AssertDependencyUnique()
    {
        // Assert dependency is in its original state
        Assert.Equal(0, _dependency.Value);

        // Modify the dependency so that a retry of this test case would fail if it got the same instance
        _dependency.Value++;
    }
}
