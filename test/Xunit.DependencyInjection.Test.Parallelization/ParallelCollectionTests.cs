namespace Xunit.DependencyInjection.Test.Parallelization;

public class ParallelCollectionTests(ConcurrencyFixture fixture) : IClassFixture<ConcurrencyFixture>
{
    [Fact]
    public Task Fact1() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact2() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact3() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact4() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact5() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact6() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact7() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact8() => fixture.CheckConcurrencyAsync();

    [Fact]
    public Task Fact9() => fixture.CheckConcurrencyAsync();
}
