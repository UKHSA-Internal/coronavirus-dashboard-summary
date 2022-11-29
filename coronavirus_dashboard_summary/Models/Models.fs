[<AutoOpen>]
module coronavirus_dashboard_summary.Models.BaseModel

open System
open System.Text.RegularExpressions
open Npgsql.FSharp
open coronavirus_dashboard_summary.Utils.TimeStamp
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Constants
open coronavirus_dashboard_summary.Models.DB
open coronavirus_dashboard_summary.Models.Metrics
open FSharp.Json

[<Literal>]
let NumberFormat = "{0:#,##0.##}"

[<Literal>]
let GeneralQuery = "
SELECT area_code, area_type, area_name, date::TEXT AS date, metric, value, priority 
FROM (
  SELECT metric, priority, area_code, ref.area_type, area_name, date,
         (
           CASE
             WHEN (payload ->> 'value') = 'UP'                          THEN 0
             WHEN (payload ->> 'value') = 'DOWN'                        THEN 180
             WHEN (payload ->> 'value') = 'SAME'                        THEN 90
             WHEN (ref.area_type = 'msoa' AND metric LIKE 'newCasesBySpecimenDate%')
               OR metric ILIKE ANY ('{%Percentage%,%Rate%}'::VARCHAR[]) THEN (payload ->> 'value')::NUMERIC
             ELSE round((payload ->> 'value')::NUMERIC)::INT
           END
         ) AS value,
         RANK() OVER ( PARTITION BY (metric) ORDER BY priority, date DESC ) AS rank
  FROM covid19.time_series_p{0}_{1} AS ts 
    JOIN covid19.release_reference AS rr  ON rr.id = release_id
    JOIN covid19.metric_reference  AS mr  ON mr.id = metric_id
    JOIN covid19.area_reference    AS ref ON ref.id = area_id
    JOIN covid19.area_priorities   AS ap  ON ref.area_type = ap.area_type
  WHERE rr.released IS TRUE
    AND ref.id = @area
    AND metric ILIKE ANY (@metrics::VARCHAR[])
) AS ts
WHERE rank = 1;"

[<Literal>]
let MsoaQuery = "
SELECT area_code, area_type, area_name, date::TEXT AS date, metric, priority,
   (
     CASE
       WHEN value::TEXT = 'UP'                    THEN 0
       WHEN value::TEXT = 'DOWN'                  THEN 180
       WHEN value::TEXT = 'SAME'                  THEN 90
       WHEN metric LIKE 'newCasesBySpecimenDate%' THEN value::NUMERIC
       ELSE round(value::NUMERIC)::INT
     END
   ) AS value
FROM (
SELECT (metric || UPPER(LEFT(key, 1)) || RIGHT(key, -1)) AS metric,
  1 AS priority,
  area_code,
  ref.area_type ,
  area_name,
  date,
  (
    CASE
      WHEN value::TEXT <> 'null' THEN TRIM(BOTH '\"' FROM value::TEXT)
      ELSE '-999999'
    END
  ) AS value,
  RANK() OVER ( PARTITION BY (metric) ORDER BY date DESC ) AS rank
FROM covid19.time_series_p{0}_{1} AS ts
  JOIN covid19.release_reference AS rr ON rr.id = release_id
  JOIN covid19.metric_reference AS mr ON mr.id = metric_id
  JOIN covid19.area_reference AS ref ON ref.id = area_id,
      jsonb_each(payload) AS pa
  WHERE rr.released IS TRUE
    AND ref.id = @area
    AND metric = 'newCasesBySpecimenDate'
  ) AS ts
WHERE ts.metric ILIKE ANY(@metrics::VARCHAR[])
AND ts.rank = 1;"

[<AbstractClass>]
type PostCodeData(redis, date, payload: PostCodeDataPayload, metrics, telemetry) =
    inherit DataBase<Payload>(redis, date, telemetry)
    
    override this.keyPrefix = "area"
    override this.keySuffix = $"{payload.id}"
           
    member this.metrics
        with get() = metrics

    member this.partition
        with get() =
            match payload.area_type with
            | AreaTypes.Overview
            | AreaTypes.Nation
            | AreaTypes.Region
            | AreaTypes.NHSRegion -> "other"
            | _ -> payload.area_type.ToLower()
            
    override this.cacheDuration = 
        {
            hours = 36
            minutes = Random().Next(0, 10)
            seconds = Random().Next(0, 60)
        }
        
    override this.queryParams () =
        [
            "@area", Sql.int payload.id
            "@metrics", Sql.stringArray this.metrics
        ]
    

type MSOAData(redis, date, payload, telemetry) =
    inherit PostCodeData(redis, date, payload, PostCodeMetrics, telemetry)
    override this.query = String.Format(GeneralQuery, date.partitionDate, this.partition)


type GeneralData(redis, date, payload, telemetry) =
    inherit PostCodeData(redis, date, payload, PostCodeMetrics, telemetry)
    override this.query = String.Format(GeneralQuery, date.partitionDate, this.partition)

type Payload with
    member inline private this.dateObj =
        match this.date :> obj with
        | :? DateTime as s -> unbox<DateTime> s
        | :? string   as s -> DateTime.Parse s
        | _                -> raise (ArgumentException())
        
    member inline this.getter (attribute: string) =
        match attribute with
        | "metric"             -> this.metric
        | "date"               -> this.dateObj
                                  |> Formatter.toIsoString
        | "formattedDate"      -> this.dateObj.ToString Generic.DateFormat
        | "7DaysAgo"           -> this.dateObj.AddDays(-7.)
                                  |> Formatter.toIsoString
        | "7DaysAgoFormatted"  -> this.dateObj.AddDays(-7.).ToString Generic.DateFormat
        | "13DaysAgoFormatted" -> this.dateObj.AddDays(-13.).ToString Generic.DateFormat
        | "6DaysAgo"           -> this.dateObj.AddDays(-6.)
                                  |> Formatter.toIsoString
        | "6DaysAgoFormatted"  -> this.dateObj.AddDays(-6.).ToString Generic.DateFormat
        | "area_code"          -> this.area_code
        | "area_type"          -> this.area_type
        | "area_name"          -> this.area_name
        | "value"              -> match this.value with
                                  | Some value -> match string value with
                                                  | Generic.NotAvailableIndicator -> Generic.NotAvailable
                                                  | _                             -> string value
                                  | _          -> String.Empty
        | "formattedValue"     -> match this.value with
                                  | Some value -> match string value with
                                                  | Generic.NotAvailableIndicator -> Generic.NotAvailable
                                                  | _                             -> String.Format(NumberFormat, value)
                                  | _          -> Generic.NotAvailable
        | "trimmedAreaName"    -> match this.area_name with
                                  | "United Kingdom" -> String.Empty
                                  | v                -> Regex.Replace(v, "(.+?)(nhs.*)?", "$1", RegexOptions.IgnoreCase)
        | _                    -> String.Empty


type PostCodeDataPayload with        
    member inline this.Data date metrics redis telemetry: Async<Payload List> =
        let fetcher =
            match this.area_type with
            | AreaTypes.MSOA -> MSOAData(redis, date, this, telemetry) :> PostCodeData
            | _              -> GeneralData(redis, date, this, telemetry) :> PostCodeData
            
        async {
            let! result = redis.GetAsync fetcher.key (fetcher :> IDatabase<Payload>).fetchFromDB   
            return match result with
                   | Some res -> Json.deserialize<Payload list>(res)
                   | _        -> []
        }
        
    member inline this.Key date redis telemetry: string =
        let fetcher =
            match this.area_type with
            | AreaTypes.MSOA -> MSOAData(redis, date, this, telemetry) :> PostCodeData
            | _              -> GeneralData(redis, date, this, telemetry) :> PostCodeData
            
        fetcher.key
