using Xunit.Sdk;

namespace Xunit.DependencyInjection.StaFact;

// ReSharper disable once InconsistentNaming
public class UITheoryTestCaseRunnerAdapter : UITestCaseRunnerAdapter
{
    /// <inheritdoc />
    public override Type TestCaseType => typeof(UITheoryTestCase);
}
