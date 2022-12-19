using xRetry;

namespace Xunit.DependencyInjection.Test;

public class xRetryTest
{
    private static int _nFact = 0;
    private static int _nTheory = 0;

    [RetryFact]
    public void RetryFact()
    {
        _nFact++;
        Assert.Equal(3, _nFact);
    }

    [RetryTheory]
    [InlineData(3)]
    public void RetryTheory(int expected)
    {
        _nTheory++;
        Assert.Equal(expected, _nTheory);
    }
}
