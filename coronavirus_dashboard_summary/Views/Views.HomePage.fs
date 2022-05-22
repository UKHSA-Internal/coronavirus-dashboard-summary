module coronavirus_dashboard_summary.Views.HomePageView 

open Giraffe
open Microsoft.ApplicationInsights
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open FSharp.Json
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Templates.Base
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Utils    
open coronavirus_dashboard_summary.Utils.TimeStamp
    
let index (date: Release) (redis: Redis.Client) =    
    let dbResp =
        [|$"area-{date.isoDate}-ENGLAND"|]
        |> redis.GetAllAsync 
        |> Async.RunSynchronously
        |> Json.deserialize<DB.Payload list>
        |> List.groupBy Filters.GroupByMetric
        |> List.map Filters.GroupByPriorityAttribute
        |> Metrics.GeneralPayload
        
    [
        yield! HomeHeading.Render
                    
        MetaData.CardMetadata
        |> Array.Parallel.map (fun metadata -> metadata.Card date dbResp null)
        |> List.concat
        |> Body.Render
    ]

let HomePageHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let release   = ReleaseTimestamp()
            let redis     = ctx.GetService<Redis.Client>()
            let telemetry = ctx.GetService<TelemetryClient>()
            
            let layout: LayoutPayload =
                {
                    date     = release
                    banners  = Banners.Render redis release telemetry
                    title    = "England Summary"
                    postcode = null
                    error    = false
                }

            return!
                index release redis
                |> layout.Render
                |> ctx.WriteHtmlViewAsync
        }
