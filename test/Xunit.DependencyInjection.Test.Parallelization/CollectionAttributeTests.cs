namespace Xunit.DependencyInjection.Test.Parallelization;

[Collection("Sequential")]
public class CollectionAttributeTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Fact]
    public async Task Fact1() => Assert.Equal(1, await fixture.CheckConcurrencyAsync());

    [Fact]
    public async Task Fact2() => Assert.Equal(1, await fixture.CheckConcurrencyAsync());
}
