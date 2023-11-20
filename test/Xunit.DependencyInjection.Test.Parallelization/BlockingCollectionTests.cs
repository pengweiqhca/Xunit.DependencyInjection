namespace Xunit.DependencyInjection.Test.Parallelization;

public class BlockingCollectionTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Fact]
    public async Task Fact1() => Assert.Equal(2, await fixture.CheckConcurrencyAsync());

    [Fact]
    public async Task Fact2() => Assert.Equal(2, await fixture.CheckConcurrencyAsync());
}
