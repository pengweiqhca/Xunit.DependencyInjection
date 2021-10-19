module XUnit.DependencyInjection.Test.FSharp.ScopedModuleTest3

open Microsoft.Extensions.DependencyInjection
open Xunit

type Dependency1 = {
    Value: string
}

type Dependency2 = {
    Value: Dependency1
}

type Dependency3 = {
    Value: Dependency2
}

let mutable private StartupThatWasUsed = null

type Startup() =
    member this.ConfigureServices(services: IServiceCollection) =
       StartupThatWasUsed <- this.GetType()
       services
           .AddSingleton({ Dependency1.Value = "ScopedModuleTest3" })
           .AddSingleton<Dependency2>()
           .AddSingleton<Dependency3>()
       |> ignore

type Test(dep: Dependency1, dep2: Dependency2, dep3: Dependency3) =

    [<Fact>]
    let ``Test that DI works in tests``() =
        Assert.Equal(dep2.Value.Value, dep.Value)
        Assert.Equal(dep3.Value.Value.Value, dep.Value)
        Assert.Equal(dep2.Value.Value, "ScopedModuleTest3")

    [<Fact>]
    let ``Test proper startup was used``() =
        Assert.Equal(typeof<Startup>, StartupThatWasUsed)
