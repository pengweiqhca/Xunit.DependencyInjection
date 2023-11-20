using xRetry;

namespace Xunit.DependencyInjection.Test;

public class XRetryTest(IDependency dependency)
{
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

    public class NonSerializableTestData(int id)
    {
        public readonly int Id = id;
    }

    [RetryTheory]
    [MemberData(nameof(GetTestData))]
    public void RetryTheoryNonSerializableData(NonSerializableTestData nonSerializableWrapper)
    {
        AssertDependencyUnique();

        _nonSerializableTheoryNumCalls[nonSerializableWrapper.Id]++;
        Assert.Equal(3, _nonSerializableTheoryNumCalls[nonSerializableWrapper.Id]);
    }

    public static IEnumerable<object[]> GetTestData() =>
    [
        [new NonSerializableTestData(0)],
        [new NonSerializableTestData(1)]
    ];

    private void AssertDependencyUnique()
    {
        // Assert dependency is in its original state
        Assert.Equal(0, dependency.Value);

        // Modify the dependency so that a retry of this test case would fail if it got the same instance
        dependency.Value++;
    }
}
