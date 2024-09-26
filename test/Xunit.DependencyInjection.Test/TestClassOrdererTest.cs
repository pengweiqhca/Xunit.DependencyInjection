namespace Xunit.DependencyInjection.Test;

public class TestClassOrdererTest
{
    public static int Count { get; set; }
}

[Collection(nameof(TestClassOrdererTest))]
public class TestClassOrdererTest1 : TestClassOrdererTest
{
    [Fact]
    public void TestOrder()
    {
        Count++;

        Assert.Equal(2, Count);
    }
}

[TestClassOrder(1)]
[Collection(nameof(TestClassOrdererTest))]
public class TestClassOrdererTest2 : TestClassOrdererTest
{
    [Fact]
    public void TestOrder()
    {
        Count++;

        Assert.Equal(1, Count);
    }
}
