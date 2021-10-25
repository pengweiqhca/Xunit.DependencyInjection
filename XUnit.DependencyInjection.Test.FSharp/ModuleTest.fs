module XUnit.DependencyInjection.Test.FSharp.ScopedModuleTest

open Microsoft.Extensions.DependencyInjection
open Xunit

let mutable private StartupThatWasUsed = null

type Startup() =
    member this.ConfigureServices(services: IServiceCollection) =
       StartupThatWasUsed <- this.GetType()
       services
           .AddScoped<_>(fun _ -> { ScopedDependency.Value = 1 })
           .AddSingleton({ Dependency1.Value = "ScopedModuleTest" })
           .AddSingleton<Dependency2>()
           .AddSingleton<Dependency3>()
       |> ignore

type Test(dep: Dependency1, dep2: Dependency2, dep3: Dependency3) =

    [<Fact>]
    let ``Test that DI works in tests``() =
        Assert.Equal(dep2.Value.Value, dep.Value)
        Assert.Equal(dep3.Value.Value.Value, dep.Value)
        Assert.Equal(dep2.Value.Value, "ScopedModuleTest")


    [<Fact>]
    let ``Test proper startup was used``() =
        Assert.Equal(typeof<Startup>, StartupThatWasUsed)

type ScopedDependencyTest1(dependency: ScopedDependency) =

    [<Fact>]
    let ``Test that scoped dependency is in fact scoped to each test``() =
        dependency.Value <- dependency.Value + 1
        Assert.Equal(2, dependency.Value)

type ScopedDependencyTest2(dependency: ScopedDependency) =

    [<Fact>]
    let ``Test that scoped dependency is in fact scoped to each test``() =
        dependency.Value <- dependency.Value + 1
        Assert.Equal(2, dependency.Value)
