namespace BikeRental.Starter

open System.IO
open BikeRental.Registration
open BikeRental.Accounting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.ApplicationParts
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.SpaServices.AngularCli

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    member self.ConfigureServices(services: IServiceCollection) =
        let registrationAssemblyPart =
            typeof<RegistrationFacade>.Assembly
            |> AssemblyPart

        let accountingAssemblyPart = typeof<AccountingId>.Assembly |> AssemblyPart

        let parts =
            services
                .AddControllers()
                .PartManager
                .ApplicationParts

        parts.Add(registrationAssemblyPart)
        parts.Add(accountingAssemblyPart)

        let facades = FacadesCreator.create self.Configuration

        services.AddSingleton<RegistrationFacade>(fun _ -> facades.Registration)
        |> ignore

        ()

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        let fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory() + "/wwwroot")
        let pathString = PathString("")

        let defaultFilesOptions = DefaultFilesOptions()
        defaultFilesOptions.RequestPath <- pathString
        defaultFilesOptions.FileProvider <- fileProvider
        defaultFilesOptions.DefaultFileNames <- [| "index.html" |]

        let staticFileOptions = StaticFileOptions()
        staticFileOptions.RequestPath <- pathString
        staticFileOptions.FileProvider <- fileProvider

        app
            .UseRouting()
            .UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore)
#if DEBUG
            .UseDeveloperExceptionPage()
            .UseSpa(fun spa ->
                spa.Options.SourcePath <- "../"
                spa.UseAngularCliServer("start")
                ())
#endif
#if RELEASE
            .UseDefaultFiles(defaultFilesOptions)
            .UseStaticFiles(staticFileOptions)
            .UseSpa(fun spa -> ())
#endif

module Program =
    [<EntryPoint>]
    let main args =
        Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder -> webBuilder.UseStartup<Startup>() |> ignore)
            .Build().Run()
        0