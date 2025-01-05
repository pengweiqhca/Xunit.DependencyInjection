namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunner(DependencyInjectionContext context)
    : XunitTestCaseRunner
{
    /// <inheritdoc />
    protected override ValueTask<RunSummary> RunTest(XunitTestCaseRunnerContext ctxt, IXunitTest test) =>
        new DependencyInjectionTestRunner(context,
            FromServicesAttribute.CreateFromServices(ctxt.TestCase.TestMethod.Method)).Run(
            test,
            ctxt.MessageBus,
            ctxt.ConstructorArguments,
            ctxt.ExplicitOption,
            ctxt.Aggregator.Clone(),
            ctxt.CancellationTokenSource,
            ctxt.BeforeAfterTestAttributes);
}
