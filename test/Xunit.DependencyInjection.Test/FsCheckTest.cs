using FsCheck.Xunit;

namespace Xunit.DependencyInjection.Test;

public class FsCheckTest
{
    [Property]
    public void PropertyTest(int value) => Assert.InRange(value, int.MinValue, int.MaxValue);
}
