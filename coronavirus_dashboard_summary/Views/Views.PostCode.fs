module coronavirus_dashboard_summary.Views.PostCodeSearch

open System
open Microsoft.ApplicationInsights
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Giraffe
open FSharp.Json
open Microsoft.AspNetCore.ResponseCaching
open StackExchange.Redis
open Npgsql.FSharp

open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Templates.Base
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Models.MetaData
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.TimeStamp
open coronavirus_dashboard_summary.Views.HomePageView
open coronavirus_dashboard_summary.Utils.Constants

let private nestedMetricsQuery (date: TimeStamp.Release) = $"\
(SELECT area_code, area_type, area_name, date::TEXT AS date, metric, payload as value, 1 as priority,
       RANK() OVER (
           PARTITION BY (metric)
           ORDER BY date DESC
           ) AS rank
FROM covid19.time_series_p{date.partitionDate}_msoa
         JOIN covid19.area_reference AS ar ON ar.id = time_series_p{date.partitionDate}_msoa.area_id
         JOIN covid19.metric_reference AS mr ON mr.id = metric_id
         JOIN covid19.release_reference AS rr ON rr.id = release_id
 WHERE area_id = @areaid
  AND date > (DATE(@date) - INTERVAL '40 days')
  AND metric = 'vaccinationsAgeDemographics'
  AND payload IS NOT NULL)
    UNION
(SELECT area_code, area_type, area_name, date::TEXT AS date, metric, payload as value, 1 as priority,
       RANK() OVER (
           PARTITION BY (metric)
           ORDER BY date DESC
           ) AS rank
FROM covid19.time_series_p{date.partitionDate}_ltla
         JOIN covid19.area_reference AS ar ON ar.id = time_series_p{date.partitionDate}_ltla.area_id
         JOIN covid19.metric_reference AS mr ON mr.id = metric_id
         JOIN covid19.release_reference AS rr ON rr.id = release_id
 WHERE area_id = @areaid
  AND date > (DATE(@date) - INTERVAL '40 days')
  AND metric = 'vaccinationsAgeDemographics'
  AND payload IS NOT NULL)
    UNION
(SELECT area_code, area_type, area_name, date::TEXT AS date, metric, payload as value, 1 as priority,
       RANK() OVER (
           PARTITION BY (metric)
           ORDER BY date DESC
           ) AS rank
FROM covid19.time_series_p{date.partitionDate}_utla
         JOIN covid19.area_reference AS ar ON ar.id = time_series_p{date.partitionDate}_utla.area_id
         JOIN covid19.metric_reference AS mr ON mr.id = metric_id
         JOIN covid19.release_reference AS rr ON rr.id = release_id
WHERE area_id = @areaid
  AND date > (DATE(@date) - INTERVAL '40 days')
  AND metric = 'vaccinationsAgeDemographics'
  AND payload IS NOT NULL)
ORDER BY rank LIMIT 1;
"

type Payload =
    {
        date:      string
        area_code: string
        area_type: string
        area_name: string
        metric:    string
        value:     string option
        priority:  int
    }

let private DBConnection =
    Sql.host (Environment.GetEnvironmentVariable "POSTGRES_HOST")
    |> Sql.database (Environment.GetEnvironmentVariable "POSTGRES_DATABASE")
    |> Sql.username (Environment.GetEnvironmentVariable "POSTGRES_USER")
    |> Sql.password (Environment.GetEnvironmentVariable "POSTGRES_PASSWORD")
    |> (fun h -> match Generic.IsDev with
                 | true -> h |> Sql.requireSslMode
                 | _    -> h)
    |> Sql.trustServerCertificate true
    |> Sql.formatConnectionString
    |> (+) "Pooling=false;"

let readMetrics (connectionString: string) (releaseDate: Release) (metrics: string[]) (area: int): Payload list =
    connectionString
    |> Sql.connect
    |> Sql.query (nestedMetricsQuery releaseDate)
    |> Sql.parameters
            [
               "@date", Sql.timestamp releaseDate.timestamp
               "@metrics", Sql.stringArray metrics
               "@areaid", Sql.int area
            ]
    |> Sql.execute (fun read ->
        {
            date = read.string "date"
            area_code = read.string "area_code"
            area_type = read.string "area_type"
            area_name = read.string "area_name"
            metric = read.string "metric"
            value = read.stringOrNone "value"
            priority = read.int "priority"
        })

type private PostCodeView(postcode, redis, telemetry) =
    let release = ReleaseTimestamp()

    // Get local areas associated with the postcode
    let postcodeAreas =
        PostCode.Model(redis, release, Validators.ValidatePostcode postcode, telemetry).PostCodeAreas
        |> Async.RunSynchronously

    member _.postCodeFound
        with get() = List.isEmpty postcodeAreas
                     |> not

    member private _.getContent (postcodeData: DB.PostCodeDataPayload list) =
        let date = ReleaseTimestamp()

        let dbArray =
            postcodeData
            |> List.map (fun item -> item.Key release redis telemetry)
            |> List.toArray

        // printfn ("%A") dbArray

        let dbRespString =
            dbArray
            |> redis.GetAllAsync
            |> Async.RunSynchronously

        let dbResp =
            dbRespString
            |> Json.deserialize<DB.Payload List>
            |> List.groupBy Filters.GroupByMetric
            |> List.map Filters.GroupByPriorityAttribute
            |> Metrics.GeneralPayload

        //printfn ("%A") dbResp

        let keyList = [for x in dbResp.Keys -> x]  // Create a list of the keys retrieved from Redis
        let nestedMetrics = [|"cumVaccinationSpring23UptakeByVaccinationDatePercentage75plus"; "cumPeopleVaccinatedSpring23ByVaccinationDate75plus"|]

        // printfn ("%A") keyList

        // Now check to see if our nested metrics are in the retrieved metrics
        if
            (List.contains "cumVaccinationSpring23UptakeByVaccinationDatePercentage75plus" keyList) ||
            (List.contains "cumPeopleVaccinatedSpring23ByVaccinationDate75plus" keyList)
        then
            printfn "%s" "Present"
        else
            printfn "%s" "Not Present"
            // let parentMetric = [|"vaccinationsAgeDemographics"|]
            //
            // let keyDate = dbArray.[0]
            // printfn ("%s") keyDate
            //
            // let results = readMetrics DBConnection date parentMetric 997
            // let nestedMetricJsonStrings = [for nestedMetric in nestedMetrics do jsonCacheString50Plus(nestedMetric, results.[0], date.isoDate)]
            // let output = String.concat ", " nestedMetricJsonStrings
            //
            // let newHead = dbRespString.Replace("]", "")
            // let newBody = newHead + ", " + output + "]"
            //
            // printfn ("%s") newBody
            //
            // let conStr = Environment.GetEnvironmentVariable "REDIS"
            // let cm = ConnectionMultiplexer.Connect conStr
            // let redisDb = cm.GetDatabase(2)
            // let keyExpiry = TimeSpan(Random().Next(3, 12), Random().Next(0, 60), Random().Next(0, 60))
            // printfn "%A" keyExpiry
            // redisDb.StringSet(RedisKey.op_Implicit keyDate, RedisValue.op_Implicit newBody, keyExpiry) |> ignore

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

                // printfn "%A" postcodeData

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
                |> Option.defaultValue String.Empty

            let redis     = ctx.GetService<Redis.Client>()
            let telemetry = ctx.GetService<TelemetryClient>()
            let view      = PostCodeView(postcode.ToUpper(), redis, telemetry)

            if not view.postCodeFound
                then ctx.SetStatusCode 404

            return! ctx.WriteHtmlViewAsync view.postcodeData
        }
