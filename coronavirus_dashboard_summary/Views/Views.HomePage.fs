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


let private nestedMetricsQuery (date: TimeStamp.Release) = $"\
    SELECT area_code, area_type, area_name, date::TEXT AS date, metric, payload as value, 1 as priority,
    RANK() OVER (
        PARTITION BY (metric)
        ORDER BY date DESC
        ) AS rank
    FROM covid19.time_series_p{date.partitionDate}_other
             JOIN covid19.area_reference AS ar ON ar.id = time_series_p{date.partitionDate}_other.area_id
             JOIN covid19.metric_reference AS mr ON mr.id = metric_id
             JOIN covid19.release_reference AS rr ON rr.id = release_id
    WHERE area_name = 'England'
      AND date > (DATE(@date) - INTERVAL '30 days')
      AND metric = 'vaccinationsAgeDemographics'
      AND payload IS NOT NULL
    ORDER BY rank LIMIT 1;
"

[<Struct; IsReadOnly>]
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

let readMetrics (connectionString: string) (releaseDate: Release) (metrics: string[]): Payload list =
    connectionString
    |> Sql.connect
    |> Sql.query (nestedMetricsQuery releaseDate)
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
            value = read.stringOrNone "value"
            priority = read.int "priority"
        })

let jsonCacheString50Plus (nestedMetric: string, dbResult: Payload, entryDate: String) =
        
    let getData data : string = match data with
                                            | None -> ""
                                            | Some data -> data
    
    let jsonInput = getData (dbResult.value)  // String is no longer optional
    let position50plus = jsonInput.IndexOf "\"age\": \"50+\""
    let string50plus = jsonInput.[position50plus..position50plus+2000]
    let positionNestedMetric = string50plus.IndexOf nestedMetric
    let stringNestedMetric = string50plus.[positionNestedMetric..positionNestedMetric+100]
    let startNestedMetric = stringNestedMetric.IndexOf ": "
    let endNestedMetric = stringNestedMetric.IndexOf ","
    let actualNestedMetric = stringNestedMetric.[startNestedMetric+2..endNestedMetric-1] 
    
    // Serialize new entry
    let dateVal = entryDate
    let dateStr = $"\"date\": \"%s{dateVal}\""
    let valueVal = actualNestedMetric
    let valueValRounded = Math.Round(float valueVal, 1)
    let valueStr = $"\"value\": %s{string valueValRounded}"
    let metricVal = nestedMetric
    let metricStr = $"\"metric\": \"%s{metricVal}\""
    let priorityVal = 5
    let priorityStr = $"\"priority\": %i{priorityVal}"
    let areacodeVal = "E92000001"
    let areacodeStr = $"\"area_code\": \"%s{areacodeVal}\""
    let areanameVal = "England"
    let areanameStr = $"\"area_name\": \"%s{areanameVal}\""
    let areatypeVal = "nation"
    let areatypeStr = $"\"area_type\": \"%s{areatypeVal}\""
    
    "{" + $"%s{dateStr}, %s{valueStr}, %s{metricStr}, %s{priorityStr}, %s{areacodeStr}, %s{areanameStr}, %s{areatypeStr}" + "}"
            

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
    let nestedMetrics = [|"cumVaccinationAutumn22UptakeByVaccinationDatePercentage"; "PeopleVaccinatedAutumn22ByVaccinationDate"|]    
    
    // Now check to see if our nested metrics are in the retrieved metrics
    if 
        (List.contains "cumVaccinationAutumn22UptakeByVaccinationDatePercentage" keyList) ||
        (List.contains "PeopleVaccinatedAutumn22ByVaccinationDate" keyList)
    then
        printfn "%s" "Present"
    else
        printfn "%s" "Not Present"
        let oldBodyLength = String.length dbRespString
        if oldBodyLength > 300 then
            let parentMetric = [|"vaccinationsAgeDemographics"|]
            let results = readMetrics DBConnection date parentMetric
            let previousDay = date.AddDays -1
            let nestedMetricJsonStrings = [for nestedMetric in nestedMetrics do jsonCacheString50Plus(nestedMetric, results.[0], (previousDay.ToString "yyyy-MM-dd"))]
            let output = String.concat ", " nestedMetricJsonStrings
            
            let newHead = dbRespString.Replace("]", "")
            let newBody = newHead + ", " + output + "]"
            
            let keyDate = $"area-{date.isoDate}-ENGLAND"
            
            let conStr = Environment.GetEnvironmentVariable "REDIS"
            let cm = ConnectionMultiplexer.Connect conStr
            let redisDb = cm.GetDatabase(2)
            let keyExpiry = TimeSpan(Random().Next(3, 12), Random().Next(0, 60), Random().Next(0, 60))
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