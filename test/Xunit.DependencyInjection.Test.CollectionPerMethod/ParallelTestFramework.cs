using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection.Test.CollectionPerMethod;

public sealed class ParallelTestFramework : XunitTestFramework
{
    public ParallelTestFramework(IMessageSink diagnosticMessageSink)
        : base(diagnosticMessageSink)
    {
    }

    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new CustomParallelTestExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);

    private sealed class CustomParallelTestExecutor : DependencyInjectionTestFrameworkExecutor
    {
        public CustomParallelTestExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            try
            {
                var newTestCases = SetUpTestCaseParallelization(testCases);

                base.RunTestCases(newTestCases, executionMessageSink, executionOptions);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// By default, all test cases in a test class share the same collection instance which ensures they run synchronously.
        /// By providing a unique test collection instance to every test case in a test class you can make them all run in parallel.
        /// </summary>
        private IEnumerable<IXunitTestCase> SetUpTestCaseParallelization(IEnumerable<IXunitTestCase> testCases)
        {

            var result = new List<IXunitTestCase>();
            foreach (var testCase in testCases)
            {
                var oldTestMethod = testCase.TestMethod;
                var oldTestClass = oldTestMethod.TestClass;
                var oldTestCollection = oldTestMethod.TestClass.TestCollection;

                // If the collection is explicitly set, don't try to parallelize test execution
                if (oldTestCollection.CollectionDefinition != null ||
                    oldTestClass.Class.GetCustomAttributes(typeof(CollectionAttribute)).Any())
                {
                    result.Add(testCase);
                    continue;
                }

                // Create a new collection with a unique id for the test case.
                var newTestCollection =
                    new TestCollection(
                        oldTestCollection.TestAssembly,
                        oldTestCollection.CollectionDefinition,
                        displayName: $"{oldTestCollection.DisplayName} {oldTestCollection.UniqueID}");

                newTestCollection.UniqueID = Guid.NewGuid();

                // Duplicate the test and assign it to the new collection
                var newTestClass = new TestClass(newTestCollection, oldTestClass.Class);

                var newTestMethod = new TestMethod(newTestClass, oldTestMethod.Method);

                switch (testCase)
                {

                    // Used by Theory having DisableDiscoveryEnumeration or non-serializable data
                    case XunitTheoryTestCase xunitTheoryTestCase:

                        result.Add(new XunitTheoryTestCase(
                            DiagnosticMessageSink,
                            GetTestMethodDisplay(xunitTheoryTestCase),
                            GetTestMethodDisplayOptions(xunitTheoryTestCase),
                            newTestMethod));

                        break;

                    // Used by all other tests
                    case XunitTestCase xunitTestCase:
                        result.Add(new XunitTestCase(
                            DiagnosticMessageSink,
                            GetTestMethodDisplay(xunitTestCase),
                            GetTestMethodDisplayOptions(xunitTestCase),
                            newTestMethod,
                            xunitTestCase.TestMethodArguments));

                        break;

                    default:

                        throw new ArgumentOutOfRangeException("Test case " + testCase.GetType() + " not supported");
                }
            }

            return testCases;

            static TestMethodDisplay GetTestMethodDisplay(TestMethodTestCase testCase) =>
                (TestMethodDisplay)typeof(TestMethodTestCase)
                    .GetProperty("DefaultMethodDisplay", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(testCase)!;

            static TestMethodDisplayOptions GetTestMethodDisplayOptions(TestMethodTestCase testCase) =>
                (TestMethodDisplayOptions)typeof(TestMethodTestCase)
                    .GetProperty("DefaultMethodDisplayOptions", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(testCase)!;
        }
    }
}
