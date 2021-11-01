module coronavirus_dashboard_summary.Views.PostCodeSearch

open Microsoft.ApplicationInsights
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Giraffe
open FSharp.Json
open Microsoft.AspNetCore.ResponseCaching
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Templates.Base
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Models.MetaData
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.TimeStamp
open coronavirus_dashboard_summary.Views.HomePageView
    
type private PostCodeView(postcode, redis, telemetry) =
    
    let release = ReleaseTimestamp()
            
    let postcodeAreas =
        PostCode.Model(redis, release, Validators.ValidatePostcode postcode, telemetry).PostCodeAreas
        |> Async.RunSynchronously
        
    member _.postCodeFound
        with get() = List.isEmpty postcodeAreas
                     |> not
        
    member private _.getContent (postcodeData: DB.PostCodeDataPayload list) = 
        let dbResp =
            postcodeData
            |> List.map (fun item -> item.Key release redis telemetry)
            |> List.toArray
            |> redis.GetAllAsync
            |> Async.RunSynchronously
            |> Json.deserialize<DB.Payload List>
            |> List.groupBy (fun item -> item.metric)
            |> List.map Filters.ByPriorityAttribute
            |> List.map (fun item -> (item.metric, item))
            |> dict
            |> Metrics.GeneralPayload

        [
            PostCodeHeading.Render postcode
            
            CardMetadata
            |> Array.Parallel.map (fun metadata -> metadata.Card release dbResp postcode)
            |> List.concat
            |> Body.Render
        ]
        
    member this.postcodeData =        
        postcodeAreas
        |> (fun postcodeData ->
                let layout: LayoutPayload =
                    {
                        date     = release
                        title    = $"Local summary for { postcode.ToUpper() }"
                        banners  = Banners.Render redis release telemetry
                        postcode = match postcodeData.IsEmpty with
                                   | true -> postcode
                                   | _ -> null
                        error    = false
                    }
                
                match postcodeData.IsEmpty with
                | true  -> index release redis
                | false -> postcodeData |> this.getContent
                |> layout.Render
        )

let PostCodePageHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let responseCachingFeature = ctx.Features.Get<IResponseCachingFeature>();

            match responseCachingFeature with
            | null -> null |> ignore
            | _    -> responseCachingFeature.VaryByQueryKeys <- [| "postcode" |]
            
            let postcode =
                ctx.TryGetQueryStringValue "postcode"
                |> Option.defaultValue ""
                
            let redis =
                ctx.GetService<Redis.Client>()
                
            let telemetry =
                ctx.GetService<TelemetryClient>()

            let view =
                PostCodeView(postcode.ToUpper(), redis, telemetry)
            
            if not view.postCodeFound
                then ctx.SetStatusCode 404

            return! ctx.WriteHtmlViewAsync view.postcodeData
        }
