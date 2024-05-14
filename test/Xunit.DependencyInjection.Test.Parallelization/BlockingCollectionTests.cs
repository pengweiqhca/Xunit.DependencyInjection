namespace Xunit.DependencyInjection.Test.Parallelization;

public class BlockingCollectionTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Fact]
    public async Task Fact1() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact2() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact3() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact4() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact5() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact6() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact7() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact8() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);

    [Fact]
    public async Task Fact9() => Assert.InRange(await fixture.CheckConcurrencyAsync(), 1, 2);
}
