namespace XUnit.DependencyInjection.Test.FSharp

open System
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

module private StartupValue =
    let mutable StartupThatWasUsed: Type = null

type Startup() =
    member this.ConfigureServices(services: IServiceCollection) =
       StartupValue.StartupThatWasUsed <- this.GetType()
       services
           .AddSingleton({ Dependency1.Value = "RootAssembly" })
           .AddSingleton<Dependency2>()
       |> ignore

type Test(dep: Dependency1, dep2: Dependency2) =

    [<Fact>]
    let ``Test that DI works in tests``() =
        Assert.Equal(dep2.Value.Value, dep.Value)
        Assert.Equal(dep2.Value.Value, "RootAssembly")

    [<Fact>]
    let ``Test proper startup was used``() =
        Assert.Equal(typeof<Startup>, StartupValue.StartupThatWasUsed)
