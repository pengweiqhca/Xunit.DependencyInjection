namespace Xunit.DependencyInjection.Test.Parallelization;

public class MemberDataAttributeTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    public static TheoryData<int> GetData() => new() { { 1 }, { 2 } };

    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public async Task Theory(int _) => Assert.Equal(1, await fixture.CheckConcurrencyAsync());
}
