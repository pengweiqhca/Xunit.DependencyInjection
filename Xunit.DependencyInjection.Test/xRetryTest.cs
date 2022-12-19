using xRetry;
using Xunit;

namespace Xunit.DependencyInjection.Test;

public class xRetryTest
{
    private readonly IDependency _dependency;
    private static int _nFact = 0;
    private static int _nTheory = 0;

    public xRetryTest(IDependency dependency)
    {
        _dependency = dependency;
    }

    [RetryFact]
    public void RetryFact()
    {
        AssertDependencyUnique();

        _nFact++;
        Assert.Equal(3, _nFact);
    }

    [RetryTheory]
    [InlineData(3)]
    public void RetryTheory(int expected)
    {
        AssertDependencyUnique();

        _nTheory++;
        Assert.Equal(expected, _nTheory);
    }

    public class NonSerializableTestData
    {
        public readonly int Id;

        public NonSerializableTestData(int id)
        {
            Id = id;
        }
    }

    // testId => numCalls
    private static readonly Dictionary<int, int> _defaultNumCalls = new()
    {
        { 0, 0 },
        { 1, 0 }
    };

    [RetryTheory]
    [MemberData(nameof(GetTestData))]
    public void RetryTheoryNonSerializableData(NonSerializableTestData nonSerializableWrapper)
    {
        AssertDependencyUnique();

        _defaultNumCalls[nonSerializableWrapper.Id]++;
        Assert.Equal(3, _defaultNumCalls[nonSerializableWrapper.Id]);
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
