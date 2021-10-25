module XUnit.DependencyInjection.Test.FSharp.ModuleTest2

open Microsoft.Extensions.DependencyInjection
open Xunit

let mutable private StartupThatWasUsed = null

type Startup() =
    member this.ConfigureServices(services: IServiceCollection) =
       StartupThatWasUsed <- this.GetType()
       services
           .AddSingleton({ Dependency1.Value = "ScopedModuleTest2" })
           .AddSingleton<Dependency2>()
       |> ignore

type Test(dep: Dependency1, dep2: Dependency2) =

    [<Fact>]
    let ``Test that DI works in tests``() =
        Assert.Equal(dep.Value, dep2.Value.Value)
        Assert.Equal("ScopedModuleTest2", dep2.Value.Value)

    [<Fact>]
    let ``Test proper startup was used``() =
        Assert.Equal(typeof<Startup>, StartupThatWasUsed)
