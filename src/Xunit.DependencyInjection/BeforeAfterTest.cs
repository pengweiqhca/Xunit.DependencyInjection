namespace Xunit.DependencyInjection;

/// <summary>
/// Base attribute which indicates a test method interception (allows code to be run before and
/// after the test is run).
/// </summary>
public abstract class BeforeAfterTest
{
    /// <summary>
    /// This method is called after the test method is executed.
    /// </summary>
    /// <param name="testClassInstance">The instance of test class</param>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual void After(object? testClassInstance, MethodInfo methodUnderTest) { }

    /// <summary>
    /// This method is called after the test method is executed.
    /// </summary>
    /// <param name="testClassInstance">The instance of test class</param>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual ValueTask AfterAsync(object? testClassInstance, MethodInfo methodUnderTest)
    {
        After(testClassInstance, methodUnderTest);

        return default;
    }

    /// <summary>
    /// This method is called before the test method is executed.
    /// </summary>
    /// <param name="testClassInstance">The instance of test class</param>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual void Before(object? testClassInstance, MethodInfo methodUnderTest) { }

    /// <summary>
    /// This method is called before the test method is executed.
    /// </summary>
    /// <param name="testClassInstance">The instance of test class</param>
    /// <param name="methodUnderTest">The method under test</param>
    public virtual ValueTask BeforeAsync(object? testClassInstance, MethodInfo methodUnderTest)
    {
        Before(testClassInstance, methodUnderTest);

        return default;
    }
}
