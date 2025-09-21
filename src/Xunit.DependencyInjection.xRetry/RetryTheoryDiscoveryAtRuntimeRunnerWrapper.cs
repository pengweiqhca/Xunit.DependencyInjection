using xRetry.v3;

#pragma warning disable IDE1006 // Naming Styles
namespace Xunit.DependencyInjection.xRetry;
#pragma warning restore IDE1006 // Naming Styles

public class RetryTheoryDiscoveryAtRuntimeRunnerWrapper : RetryTestCaseRunnerWrapper
{
    /// <inheritdoc />
    public override Type TestCaseType => typeof(RetryTheoryDiscoveryAtRuntimeCase);
}
