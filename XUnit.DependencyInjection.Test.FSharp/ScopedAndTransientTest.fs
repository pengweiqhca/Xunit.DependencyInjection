module XUnit.DependencyInjection.Test.FSharp.ScopedAndTransientTest

open Xunit

type ScopedDependencyTest1(dependency: ScopedDependency) =

    [<Fact>]
    let ``Test that scoped dependency is in fact scoped to each test``() =
        dependency.Value <- dependency.Value + 1
        Assert.Equal(1, dependency.Value)

type ScopedDependencyTest2(dependency: ScopedDependency) =

    [<Fact>]
    let ``Test that scoped dependency is in fact scoped to each test``() =
        dependency.Value <- dependency.Value + 1
        Assert.Equal(1, dependency.Value)
