namespace Xunit.DependencyInjection.Test;

public class TheoryTest
{
    [Theory]
    [MemberData(nameof(GetComplexData))]
    public void ComplexParameterizedTest(string arg1, Dictionary<string, string> arg2, Dictionary<string, string> arg3, int delay)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal("Value", arg2["Key"]);
        Assert.Equal("Value", arg3["Key"]);
        Assert.True(delay >= 0);
    }

    [Theory]
    [MemberData(nameof(GetComplexData))]
    public async Task ComplexParameterizedTestAsync(string arg1, Dictionary<string, string> arg2, Dictionary<string, string> arg3, int delay)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal("Value", arg2["Key"]);
        Assert.Equal("Value", arg3["Key"]);
        Assert.True(delay >= 0);

        await Task.Delay(delay).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(GetSimpleData))]
    public void SimpleParameterizedTest(string arg1, int arg2, int delay)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(1, arg2);
        Assert.True(delay >= 0);
    }

    [Theory]
    [MemberData(nameof(GetSimpleData))]
    public void SimpleParameterizedTestAsync(string arg1, int arg2, int delay)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(1, arg2);
        Assert.True(delay >= 0);
    }

    [Theory]
    [InlineData("Test", 1)]
    public void InlineDataTest(string arg1, int arg2)
    {
        Assert.Equal("Test", arg1);
        Assert.Equal(1, arg2);
    }

    public static IEnumerable<object[]> GetSimpleData()
    {
        yield return new object[]
        {
            "Test", 1, 0
        };
        yield return new object[]
        {
            "Test", 1, 10
        };
        yield return new object[]
        {
            "Test", 1, 1
        };
    }

    public static IEnumerable<object[]> GetComplexData()
    {
        yield return new object[]
        {
            "Test",
            new Dictionary<string, string>
            {
                { "Key", "Value"}
            },
            new Dictionary<string, string>
            {
                { "Key", "Value"}
            },
            0
        };

        yield return new object[]
        {
            "Test",
            new Dictionary<string, string>
            {
                { "Key", "Value"}
            },
            new Dictionary<string, string>
            {
                { "Key", "Value"}
            },
            10
        };

        yield return new object[]
        {
            "Test",
            new Dictionary<string, string>
            {
                { "Key", "Value"}
            },
            new Dictionary<string, string>
            {
                { "Key", "Value"}
            },
            1
        };
    }
}
