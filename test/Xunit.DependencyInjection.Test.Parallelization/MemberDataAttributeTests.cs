namespace Xunit.DependencyInjection.Test.Parallelization;

public class MemberDataAttributeTests : IClassFixture<ConcurrencyFixture>
{
    private readonly ConcurrencyFixture _fixture;

    public static TheoryData<int> GetData() => new() { { 1 }, { 2 } };

    public MemberDataAttributeTests(ConcurrencyFixture fixture) => this._fixture = fixture;

    [Theory]
    [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
    public async Task Theory(int _) => Assert.Equal(1, await _fixture.CheckConcurrencyAsync().ConfigureAwait(false));
}
