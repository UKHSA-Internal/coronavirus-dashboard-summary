module coronavirus_dashboard_summary.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.ResponseCaching
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open coronavirus_dashboard_summary.Views
open coronavirus_dashboard_summary.Utils


let webApp =
    choose [
        GET >=>
            choose [
                route "/"
                >=> publicResponseCaching 60 None
                >=> HomePageView.HomePageHandler
                
                route "/search"
                >=> publicResponseCaching 120 None
                >=> PostCodeSearch.PostCodePageHandler
                
                route "/healthcheck"
                >=> text "Healthy"
            ]
        RequestErrors.NOT_FOUND "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    
    clearResponse
    >=> setStatusCode 500
    >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001"
        )
       .WithMethods("GET", "HEAD")
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
            .UseCors(configureCors)
            .UseStaticFiles()
            .UseResponseCaching()
            .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services .AddCors()
             .AddResponseCaching(fun (options: ResponseCachingOptions) ->
                 options.MaximumBodySize <- 16 * 1024 * 1024 |> int64;
                 options.UseCaseSensitivePaths <- false
             )
             .AddGiraffe()
             .AddScoped<Redis.Client>() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0