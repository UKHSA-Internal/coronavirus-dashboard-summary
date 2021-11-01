module coronavirus_dashboard_summary.Models.HomePageModel

open System
open Npgsql.FSharp
open coronavirus_dashboard_summary.Models.DB
open coronavirus_dashboard_summary.Utils
open FSharp.Json
    
let private homePageQuery (date: TimeStamp.Release) = $"\
SELECT
    area_code, area_type, area_name, date::TEXT AS date, metric, value, 1 AS priority
FROM (
    SELECT area_code,
         MAX(area_type)   AS area_type,
         MAX(area_name)   AS area_name,
         MAX(date)        AS date,
         metric,
         MAX(
             CASE
                WHEN (payload ->> 'value')::TEXT = 'UP'   THEN 0
                WHEN (payload ->> 'value')::TEXT = 'DOWN' THEN 180
                WHEN (payload ->> 'value')::TEXT = 'SAME' THEN 90
                ELSE (payload ->> 'value')::NUMERIC
            END
         ) AS value,
         RANK() OVER (
            PARTITION BY (metric)
            ORDER BY date DESC
         ) AS rank
    FROM covid19.time_series_p{date.partitionDate}_other   AS main
    JOIN covid19.release_reference AS rr ON rr.id = release_id
    JOIN covid19.metric_reference  AS mr ON mr.id = metric_id
    JOIN covid19.area_reference    AS ar ON ar.id = main.area_id
    WHERE
          area_type = 'overview'
      AND date > ( DATE(@date) - INTERVAL '30 days' )
      AND metric = ANY( @metrics::VARCHAR[] )
    GROUP BY area_type, area_code, date, metric
) AS result
WHERE result.rank = 1;"

let formatDate (d: DateTime) =
    d.ToString "d MMMM yyyy"
    
let subtractFormatDate (d: DateTime) (n: float) =
    d.AddDays n |> formatDate
   
let private fetch (redis: Redis.Client) (date: TimeStamp.Release) (metrics: string []): Async<string option> =
    async {
        let! result =
            Sql.host (Environment.GetEnvironmentVariable "POSTGRES_HOST")
            |> Sql.database (Environment.GetEnvironmentVariable "POSTGRES_DATABASE")
            |> Sql.username (Environment.GetEnvironmentVariable "POSTGRES_USER")
            |> Sql.password (Environment.GetEnvironmentVariable "POSTGRES_PASSWORD")
            |> Sql.requireSslMode
            |> Sql.trustServerCertificate true
            |> Sql.formatConnectionString
            |> Sql.connect
            |> Sql.query (homePageQuery date) 
            |> Sql.parameters
                   [
                       "@date", Sql.timestamp date.timestamp
                       "@metrics", Sql.stringArray metrics
                   ]
            |> Sql.executeAsync
                (fun read ->
                      {
                          area_type = read.string "area_type"
                          area_code = read.string "area_code"
                          area_name = read.string "area_name"
                          date      = read.string "date"
                          metric    = read.string "metric"
                          value     = read.doubleOrNone "value"
                          priority  = read.int "priority"
                      }
                )
            |> Async.AwaitTask
            
        return! redis.SetAsync
                    $"area-{date.isoDate}-UK"
                    result
                    {
                        hours   = Random().Next(3, 12)
                        minutes = Random().Next(0, 60)
                        seconds = Random().Next(0, 60)
                    } 
    }

let Data (date: TimeStamp.Release) (metrics: string []) (redis: Redis.Client): Async<Payload List> =
    async {
        let dbRes () = fetch redis date metrics
        let! result = redis.GetAsync $"area-{date.isoDate}-UK" dbRes
        
        return match result with
               | Some res -> Json.deserialize<Payload list>(res)
               | _ -> []
    }
