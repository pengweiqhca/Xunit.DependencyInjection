namespace Xunit.DependencyInjection.Test.Parallelization;

[DisableParallelization]
public class SequentialCollectionTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Fact]
    public async Task Fact1() => Assert.Equal(1, await fixture.CheckConcurrencyAsync());

    [Fact]
    public async Task Fact2() => Assert.Equal(1, await fixture.CheckConcurrencyAsync());
}
