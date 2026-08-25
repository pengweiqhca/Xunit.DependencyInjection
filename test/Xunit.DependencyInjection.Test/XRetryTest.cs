using xRetry.v3;

namespace Xunit.DependencyInjection.Test;

public class XRetryTest
{
    public XRetryTest(IDependency _) { }

    private static int _factNumCalls;

    // testId => numCalls
    private static readonly Dictionary<int, int> TheoryNumCalls = new()
    {
        { 0, 0 },
        { 1, 0 }
    };

    // testId => numCalls
    private static readonly Dictionary<int, int> NonSerializableTheoryNumCalls = new()
    {
        { 0, 0 },
        { 1, 0 }
    };

    [RetryFact]
    public void RetryFact()
    {
        _factNumCalls++;
        Assert.Equal(3, _factNumCalls);
    }

    [RetryTheory]
    [InlineData(0)]
    [InlineData(1)]
    public void RetryTheory(int id)
    {
        TheoryNumCalls[id]++;
        Assert.Equal(3, TheoryNumCalls[id]);
    }

    public class NonSerializableTestData(int id)
    {
        public readonly int Id = id;
    }

    [RetryTheory]
    [MemberData(nameof(GetTestData))]
    public void RetryTheoryNonSerializableData(NonSerializableTestData nonSerializableWrapper)
    {
        NonSerializableTheoryNumCalls[nonSerializableWrapper.Id]++;
        Assert.Equal(3, NonSerializableTheoryNumCalls[nonSerializableWrapper.Id]);
    }

    public static IEnumerable<object[]> GetTestData() =>
    [
        [new NonSerializableTestData(0)],
        [new NonSerializableTestData(1)]
    ];
}
