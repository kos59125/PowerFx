namespace PowerFxApp.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting

module Program =

    [<EntryPoint>]
    [<CompiledName("Main")>]
    let main args =
        let builder =
            WebAssemblyHostBuilder.CreateDefault(args)

        builder.RootComponents.Add<Application.ApplicationComponent>("#main")
        builder.Build().RunAsync() |> ignore
        0
