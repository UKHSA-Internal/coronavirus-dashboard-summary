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
        hours: int
        minutes: int
        seconds: int
    }
    
type Client (telemetry: TelemetryClient) = 
    let mutable errCount = Array.init RedisPoolSize int
        
    let cxp = ConnectionMultiplexerPoolFactory
                  .Create(RedisPoolSize, RedisConfig, null, ConnectionSelectionStrategy.RoundRobin)
      
    member inline private _.QueryRedisAsync (op: IDatabase -> Async<'a>) =
        async {
            let! cx = cxp.GetAsync() |> Async.AwaitTask
            
            try
                return! op(cx.Connection.GetDatabase(RedisDatabase))
            with
            | :? RedisConnectionException as ex when errCount.[cx.ConnectionIndex] + 1 < 3 -> return! raise(ex)
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
            let! _ = this.QueryRedisAsync (fun (db: IDatabase) ->
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
            
            return Some(data.ToString())
        }
        
    member this.SetOverrideAsync (key: string) (value: 'TO list) (expiry: Expiry): Async<string option> =
        let conf = JsonConfig.create(unformatted = true)
        let data = Json.serializeEx conf value

        async {
            let! _ = this.QueryRedisAsync (fun (db: IDatabase) ->
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
            
            return Some(data.ToString())
        }
        
    member this.GetAsync (key: string): Async<string option> =
        async {

            let! result = this.QueryRedisAsync (fun (db: IDatabase) ->
                db.StringGetAsync(RedisKey key)
                |> Async.AwaitTask
            )
            
            return match result.IsNullOrEmpty with
                   | false -> Some(result.ToString())
                   | true -> None
                        
        }
        
    member this.GetAllAsync (keys: string[]): Async<string> =
        async {
                
            let! result = this.QueryRedisAsync (fun (db: IDatabase) ->
                keys
                |> Array.map(RedisKey)
                |> db.StringGetAsync
                |> Async.AwaitTask
            )
            
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
            let startTime = DateTimeOffset.UtcNow
            let swFlush = Stopwatch.StartNew()
            
            let! result = this.QueryRedisAsync (fun (db: IDatabase) ->
                db.HashGetAsync(RedisKey key, RedisValue field)
                |> Async.AwaitTask
            )
            
            swFlush.Stop()
            
            let tracker = DependencyTelemetry
                              (
                                  "Redis",
                                  RedisHostName,
                                  "HSET",
                                  String.Empty,
                                  startTime,
                                  swFlush.Elapsed,
                                  "200",
                                  true
                              )
                        
            telemetry.TrackDependency(tracker)
            
            return match result.IsNullOrEmpty with
                   | false -> Some(result.ToString())
                   | true -> None
                        
        }

    member this.SetHashAsync (key: string) (field: string) (value: 'TO list): Async<string option> =
        let conf = JsonConfig.create(unformatted = true)
        let data = Json.serializeEx conf value

        async {
            let startTime = DateTimeOffset.UtcNow
            let swFlush = Stopwatch.StartNew()
            
            let! _ = this.QueryRedisAsync (fun (db: IDatabase) ->
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
            swFlush.Stop()
            
            let tracker = DependencyTelemetry
                              (
                                  "Redis",
                                  RedisHostName,
                                  "HSET",
                                  String.Empty,
                                  startTime,
                                  swFlush.Elapsed,
                                  "200",
                                  true
                              )
            
            telemetry.TrackDependency(tracker)
            
            return Some(data.ToString())
        }
