module coronavirus_dashboard_summary.Models.DB

open System
open System.Diagnostics
open System.Runtime.CompilerServices
open FSharp.Json
open Npgsql.FSharp
open Npgsql
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Constants
open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.DataContracts

[<Struct; IsReadOnly>]
type PostCodeDataPayload =
    {
        id:        int
        area_type: string
        area_name: string
        postcode:  string
        priority:  int
    }       

type DateTransform() =
    interface ITypeTransform with
        member this.targetType() = typeof<String>
        
        member this.toTargetType obj =
            match obj with
            | :? string   as s -> DateTime.Parse s |> box
            | :? DateTime as s -> s |> box
            | :? int64    as s -> DateTimeOffset.FromUnixTimeSeconds(s) |> box
            | _ -> raise (ArgumentException())
            
        member this.fromTargetType obj =
            match obj with
            | :? DateTime as s -> (Formatter.toIsoDate (unbox<DateTime> s)) :> obj 
            | :? int64    as s -> DateTimeOffset.FromUnixTimeSeconds(s) :> obj
            | :? string   as s -> s :> obj
            | _ -> raise (ArgumentException())
            
        
[<Struct; IsReadOnly>]
type Payload =
    {
        [<JsonField("date", Transform=typeof<DateTransform>)>]
        date:      string
        area_code: string
        area_type: string
        area_name: string
        metric:    string
        value:     double option
        priority:  int
    }
    
[<Struct; IsReadOnly>]
type ChangeLogPayload =
    {
        id:            string
        date:          DateTime
        high_priority: bool
        tag:           string
        heading:       string
        body:          string
    }
 
[<Struct; IsReadOnly>]
type AnnouncementPayload =
    {
        date:   DateTime
        body:   string
    }
        
[<IsReadOnly>]
type Tracker =
    {
        Success: unit -> unit
        Failure: Exception -> Exception
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
    
type IDatabase<'T> =
    abstract member fetchFromDB: unit -> Async<string option>
    
    
[<AbstractClass>]
type DataBase<'T>(redis: Redis.Client, date: TimeStamp.Release, telemetry: TelemetryClient) =    
    member private this.startTelemetry (payload: string) =
        let startTime = DateTimeOffset.UtcNow
        let swFlush = Stopwatch.StartNew()
            
        {
             Success = fun () ->
                swFlush.Stop()
                let tracker = DependencyTelemetry
                                  (
                                      "postgresql",
                                      "database",
                                      "query",
                                      payload,
                                      startTime,
                                      swFlush.Elapsed,
                                      "200",
                                      true
                                  )
                                  
                telemetry.TrackDependency(tracker)
                
             Failure = fun ex ->
                swFlush.Stop()
                
                let tracker = DependencyTelemetry
                                  (
                                      "postgresql",
                                      "database",
                                      "query",
                                      payload,
                                      startTime,
                                      swFlush.Elapsed,
                                      "500",
                                      false
                                  )
                
                telemetry.TrackDependency tracker
                telemetry.TrackException ex
                
                ex
        }
            
    abstract query: string
        with get
            
    abstract member queryParams: unit -> (string * SqlValue) list
    
    abstract member keyPrefix: string
    
    abstract member keySuffix: string
    
    abstract member cacheDuration: Redis.Expiry
    
    abstract member key: string
    
    default this.keyPrefix = "area"
    
    default this.keySuffix = ""
    
    default this.key = $"{this.keyPrefix}-{this.date.isoDate}-{this.keySuffix}"

    interface IDatabase<Payload> with
        member this.fetchFromDB () =
            async {
                let tracker = this.startTelemetry this.query
                
                try
                    let! result =
                        this.preppedQuery
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

                    tracker.Success()
                    
                    return! redis.SetAsync
                                this.key
                                result
                                this.cacheDuration
                with
                | :? PostgresException as ex -> return! raise (tracker.Failure (ex :> Exception))
                | :? NpgsqlException   as ex -> return! raise (tracker.Failure (ex :> Exception))
            }
            
    interface IDatabase<PostCodeDataPayload> with
        member this.fetchFromDB () =
            async {
                let tracker = this.startTelemetry this.query
                
                try 
                    let! result =
                        this.preppedQuery
                        |> Sql.executeAsync
                            (fun read ->
                                {
                                    id = read.int "id"
                                    area_type = read.string "area_type"
                                    area_name = read.string "area_name"
                                    postcode  = read.string "postcode"
                                    priority  = read.int "priority"
                                }
                            )
                        |> Async.AwaitTask
                    
                    tracker.Success()

                    return! redis.SetHashAsync
                                this.key
                                this.keySuffix
                                result
                with
                | :? PostgresException as ex -> return! raise (tracker.Failure (ex :> Exception))
                | :? NpgsqlException   as ex -> return! raise (tracker.Failure (ex :> Exception))
            }

    interface IDatabase<ChangeLogPayload> with
        member this.fetchFromDB () : Async<string option> =            
            async {
                let tracker = this.startTelemetry this.query
                
                try
                    let! result =
                        this.preppedQuery
                        |> Sql.executeAsync
                            (fun read ->
                                {
                                    id            = read.string "id"
                                    date          = read.dateTime "date"
                                    high_priority = read.bool "high_priority"
                                    tag           = read.string "tag"
                                    heading       = read.string "heading"
                                    body          = read.string "body"
                                }
                            )
                        |> Async.AwaitTask
                        
                    tracker.Success()

                    return! redis.SetAsync
                                this.key
                                result
                                this.cacheDuration
                with
                | :? PostgresException as ex -> return! raise (tracker.Failure (ex :> Exception))
                | :? NpgsqlException   as ex -> return! raise (tracker.Failure (ex :> Exception))
            }
            
    interface IDatabase<AnnouncementPayload> with
        member this.fetchFromDB () =
            async {
                let tracker = this.startTelemetry this.query
                
                try
                    let! result = 
                        this.preppedQuery
                        |> Sql.executeAsync
                            (fun read ->
                                {
                                    date = read.dateTime "date"
                                    body = read.text "body"
                                }
                            )
                        |> Async.AwaitTask

                    tracker.Success()

                    return! redis.SetAsync
                                this.key
                                result
                                this.cacheDuration
                with
                | :? PostgresException as ex -> return! raise (tracker.Failure (ex :> Exception))
                | :? NpgsqlException   as ex -> return! raise (tracker.Failure (ex :> Exception))
            }
        
    member this.date
        with get(): TimeStamp.Release = date
        
    member this.redis
        with get() = redis

    member private this.preppedQuery
        with get() =
            DBConnection
            |> Sql.connect
            |> Sql.query this.query 
            |> Sql.parameters (this.queryParams())
