module coronavirus_dashboard_summary.Views.HomePageView

open Giraffe
open Microsoft.ApplicationInsights
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open FSharp.Json
open System
open System.Runtime.CompilerServices
open StackExchange.Redis
open Npgsql.FSharp
open Newtonsoft.Json.Linq

open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Templates.Base
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.TimeStamp
open coronavirus_dashboard_summary.Utils.Constants


// TODO: Used the 'metrics' parameter instead of hardcoded list
let private selectedNestedMetricsQuery (date: TimeStamp.Release) = $"\
    SELECT area_code, area_type, area_name, date::TEXT AS date, metric, (payload -> 'value')::TEXT AS value, 1 as priority,
    RANK() OVER (
        PARTITION BY (metric)
        ORDER BY date DESC
        ) AS rank
    FROM covid19.time_series_p{date.partitionDate}_other
        JOIN covid19.area_reference AS ar ON ar.id = covid19.time_series_p{date.partitionDate}_other.area_id
        JOIN covid19.metric_reference AS mr ON mr.id = metric_id
        JOIN covid19.release_reference AS rr ON rr.id = release_id
    WHERE area_name = 'England'
      AND date > (DATE(@date) - INTERVAL '40 days')
      AND metric IN ('cumVaccinationSpring23UptakeByVaccinationDatePercentage75plus', 'cumPeopleVaccinatedSpring23ByVaccinationDate75plus')
      AND payload IS NOT NULL
    ORDER BY rank LIMIT 2;
"

[<Struct; IsReadOnly>]
type Payload =
    {
        date:      string
        area_code: string
        area_type: string
        area_name: string
        metric:    string
        value:     string
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

let readMetrics (connectionString: string) (releaseDate: Release) (metrics: string[]): Payload list =
    connectionString
    |> Sql.connect
    |> Sql.query (selectedNestedMetricsQuery releaseDate)
    |> Sql.parameters
            [
               "@date", Sql.timestamp releaseDate.timestamp
               "@metrics", Sql.stringArray metrics
            ]
    |> Sql.execute (fun read ->
        {
            date = read.string "date"
            area_code = read.string "area_code"
            area_type = read.string "area_type"
            area_name = read.string "area_name"
            metric = read.string "metric"
            value = read.string "value"
            priority = read.int "priority"
        })

let dbDataToString(results) =
    [for result in results ->
        printfn "result.date: %A" result.date
        printfn "result.area_code: %A" result.area_code
        printfn "result.area_type: %A" result.area_type
        printfn "result.area_name: %A" result.area_name
        printfn "result.metric: %A" result.metric
        printfn "result.value: %A" result.value
        printfn "result.priority: %A" result.priority

        // Serialize new entry
        let dateStr = $"\"date\": \"%s{result.date}\""
        let areacodeStr = $"\"area_code\": \"%s{result.area_code}\""
        let areatypeStr = $"\"area_type\": \"%s{result.area_type}\""
        let areanameStr = $"\"area_name\": \"%s{result.area_name}\""
        let metricStr = $"\"metric\": \"%s{result.metric}\""
        let valueValRounded = Math.Round(float result.value, 1)
        let valueStr = $"\"value\": %s{string valueValRounded}"

        let priorityStr = $"\"priority\": %i{result.priority}"

        "{" + $"%s{dateStr}, %s{areacodeStr}, %s{areatypeStr}, %s{areanameStr}, %s{metricStr}, %s{valueStr}, %s{priorityStr}" + "}"
    ]

let index (date: Release) (redis: Redis.Client) =

    let dbRespString =
        [|$"area-{date.isoDate}-ENGLAND"|]
        |> redis.GetAllAsync
        |> Async.RunSynchronously

    let dbResp =
        dbRespString
        |> Json.deserialize<DB.Payload list>
        |> List.groupBy Filters.GroupByMetric
        |> List.map Filters.GroupByPriorityAttribute
        |> Metrics.GeneralPayload

    let keyList = [for x in dbResp.Keys -> x]  // Create a list of the keys retrieved from Redis
    let nestedMetrics = [|"cumVaccinationSpring23UptakeByVaccinationDatePercentage75plus"; "cumPeopleVaccinatedSpring23ByVaccinationDate75plus"|]

    // Now check to see if our nested metrics are in the retrieved metrics
    if
        (List.contains "cumVaccinationSpring23UptakeByVaccinationDatePercentage75plus" keyList) ||
        (List.contains "cumPeopleVaccinatedSpring23ByVaccinationDate75plus" keyList)
    then
        printfn "%s" "Present"
    else
        printfn "%s" "Not Present"
        let oldBodyLength = String.length dbRespString
        if oldBodyLength > 300 then
            let results = readMetrics DBConnection date nestedMetrics
            let previousDay = date.AddDays -1
            let nestedMetricJsonStrings = dbDataToString(results)
            let output = String.concat ", " nestedMetricJsonStrings

            let newHead = dbRespString.Replace("]", "")
            let newBody = newHead + ", " + output + "]"

            let keyDate = $"area-{date.isoDate}-ENGLAND"

            let conStr = Environment.GetEnvironmentVariable "REDIS"
            let cm = ConnectionMultiplexer.Connect conStr
            let redisDb = cm.GetDatabase(2)
            let keyExpiry = TimeSpan(Random().Next(670, 700), Random().Next(0, 60), Random().Next(0, 60))
            let result =
                try
                    JArray.Parse(newBody) |> ignore
                    let newBodyLength = String.length newBody
                    if newBodyLength > 300 then
                        redisDb.StringSet(RedisKey.op_Implicit keyDate, RedisValue.op_Implicit newBody, keyExpiry) |> ignore
                    else
                        printfn("New JSON body is too short. Not saving.")
                with
                    | :? Newtonsoft.Json.JsonReaderException -> printfn "Badly formed JSON. Not saving";
            result |> ignore

        else
            printfn("!!!!! Response from Redis too short. Not saving additional data.")
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
            let databaseResponse = index release redis
            return!
                databaseResponse
                |> layout.Render
                |> ctx.WriteHtmlViewAsync
        }