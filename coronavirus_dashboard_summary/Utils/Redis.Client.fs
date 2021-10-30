module coronavirus_dashboard_summary.Utils.Redis

open System
open System.Diagnostics
open System.Runtime.CompilerServices
open Giraffe
open StackExchange.Redis
open StackExchange.Redis.MultiplexerPool
open FSharp.Json
open Microsoft.ApplicationInsights
open Microsoft.ApplicationInsights.DataContracts


[<Literal>]
let private RedisDatabase = 2

[<Literal>]
let private RedisPoolSize = 30

let private conStr = Environment.GetEnvironmentVariable "REDIS"

let private RedisConfig = ConfigurationOptions.Parse(conStr)

let private RedisHostName = (conStr.Split ".").[0]

[<Struct; IsReadOnly>]
type Expiry =
    {
        hours:   int
        minutes: int
        seconds: int
    }
    
    
[<IsReadOnly>]
type private Tracker =
    {
        Success: unit -> unit
        Failure: RedisException -> RedisException
    }
    
type Client (telemetry: TelemetryClient) = 
    let mutable errCount = Array.init RedisPoolSize int
        
    let cxp = ConnectionMultiplexerPoolFactory
                  .Create(RedisPoolSize, RedisConfig, null, ConnectionSelectionStrategy.RoundRobin)
                  
    member private this.startTelemetry (cmd: string) (payload: string) =
        let startTime = DateTimeOffset.UtcNow
        let swFlush = Stopwatch.StartNew()
            
        {
             Success = fun () ->
                swFlush.Stop()
                let tracker = DependencyTelemetry
                                  (
                                      "Redis",
                                      RedisHostName,
                                      $"{cmd} ${payload}",
                                      String.Empty,
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
                                      "Redis",
                                      RedisHostName,
                                      $"{cmd} {payload}",
                                      ex.ToString(),
                                      startTime,
                                      swFlush.Elapsed,
                                      "500",
                                      false
                                  )            
                telemetry.TrackDependency tracker
                telemetry.TrackException ex
                ex
        }
            
    member inline private _.QueryRedisAsync (tracker: Tracker) (op: IDatabase -> Async<'a>) =
        async {
            let! cx = cxp.GetAsync() |> Async.AwaitTask
            
            try
                return! op(cx.Connection.GetDatabase(RedisDatabase))
            with
            | :? RedisConnectionException as ex when errCount.[cx.ConnectionIndex] + 1 < 3 -> return! raise(ex)
            | :? RedisException as ex -> return! raise (tracker.Failure(ex))
            | _ -> 
                errCount.[cx.ConnectionIndex] <- errCount.[cx.ConnectionIndex] + 1
                    
                cx.ReconnectAsync()
                |> Async.AwaitTask
                |> ignore
                
                return! op(cx.Connection.GetDatabase())
        }
            
    member this.SetAsync (key: string) (value: 'TO list) (expiry: Expiry): Async<string option> =
        let conf = JsonConfig.create(unformatted = true)
        let data = Json.serializeEx conf value

        async {

            let tracker = this.startTelemetry "SET" key

            let! _ = this.QueryRedisAsync tracker (fun (db: IDatabase) ->
                db.StringSetAsync
                    (
                        RedisKey key,
                        RedisValue data,
                        TimeSpan(expiry.hours, expiry.minutes, expiry.seconds),
                        When.NotExists,
                        CommandFlags.FireAndForget
                    )
                |> Async.AwaitTask
            )
            
            tracker.Success()
            
            return Some(data.ToString())
        }
        
    member this.SetOverrideAsync (key: string) (value: 'TO list) (expiry: Expiry): Async<string option> =
        let conf = JsonConfig.create(unformatted = true)
        let data = Json.serializeEx conf value

        async {
            
            let tracker = this.startTelemetry "SET NX" key
            
            let! _ = this.QueryRedisAsync tracker (fun (db: IDatabase) ->
                db.StringSetAsync
                    (
                        RedisKey key,
                        RedisValue data,
                        TimeSpan(expiry.hours, expiry.minutes, expiry.seconds),
                        When.NotExists,
                        CommandFlags.None
                    )
                |> Async.AwaitTask
            )
            
            tracker.Success()
            
            return Some(data.ToString())
            
        }
        
    member this.GetAsync (key: string): Async<string option> =
        async {

            let tracker = this.startTelemetry "GET" key

            let! result = this.QueryRedisAsync tracker (fun (db: IDatabase) ->
                db.StringGetAsync(RedisKey key)
                |> Async.AwaitTask
            )
            
            tracker.Success()
            
            return match result.IsNullOrEmpty with
                   | false -> Some(result.ToString())
                   | true -> None
                        
        }
        
    member this.GetAllAsync (keys: string[]): Async<string> =
        async {
            
            let tracker = this.startTelemetry "GET" ( String.Join(" ", keys) )

            let! result = this.QueryRedisAsync tracker (fun (db: IDatabase) ->
                keys
                |> Array.map(RedisKey)
                |> db.StringGetAsync
                |> Async.AwaitTask
            )
            
            tracker.Success()
            
            let response =
                result
                |> Array.map (
                        fun item ->
                            match item.IsNullOrEmpty with
                            | false -> item.ToString().Trim('[', ']')
                            | true -> ""
                        )
                |> Array.filter (fun item -> String.IsNullOrEmpty item |> not)
                |> String.concat ","
            
            return $"[{response}]"
                        
        }
        
    member this.GetHashAsync (key: string) (field: string): Async<string option> =
        async {
            
            let tracker = this.startTelemetry "HGET" key
            
            let! result = this.QueryRedisAsync tracker (fun (db: IDatabase) ->
                db.HashGetAsync(RedisKey key, RedisValue field)
                |> Async.AwaitTask
            )
            
            tracker.Success()
            
            return match result.IsNullOrEmpty with
                   | false -> Some(result.ToString())
                   | true -> None
                        
        }

    member this.SetHashAsync (key: string) (field: string) (value: 'TO list): Async<string option> =
        let conf = JsonConfig.create(unformatted = true)
        let data = Json.serializeEx conf value

        async {
            
            let tracker = this.startTelemetry "HSET" key
            
            let! _ = this.QueryRedisAsync tracker (fun (db: IDatabase) ->
                db.HashSetAsync
                    (
                        RedisKey key,
                        RedisValue field,
                        RedisValue data,
                        When.NotExists,
                        CommandFlags.FireAndForget
                    )
                |> Async.AwaitTask
            )
            
            tracker.Success()
            
            return Some(data.ToString())
            
        }
