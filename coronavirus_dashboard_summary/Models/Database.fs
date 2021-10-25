module coronavirus_dashboard_summary.Models.DB

open System
open coronavirus_dashboard_summary.Utils
open Npgsql.FSharp
open FSharp.Json

[<Struct>]
type PostCodeDataPayload =
    {
        id: int
        area_type: string
        area_name: string
        postcode: string
        priority: int
    }       
   
   
type x = DateTime | String | Int64
   
type DateTransform() =
    interface ITypeTransform with
        member this.targetType() = typeof<String> // typeof<x> //typeof<DateTime> 
        
        member this.toTargetType obj =
            match obj with
            | :? string as s -> DateTime.Parse s |> box
            | :? DateTime as s -> s |> box
            | :? int64  as s -> DateTimeOffset.FromUnixTimeSeconds(s) |> box
            | _ -> raise (ArgumentException())
            
        member this.fromTargetType obj =
            match obj with
            | :? DateTime as s -> (Formatter.toIsoDate (unbox<DateTime> s)) :> obj 
            | :? int64 as s -> DateTimeOffset.FromUnixTimeSeconds(s) :> obj // (Formatter.toIsoDate (unbox<DateTime> s)) :> obj 
            | :? string as s -> s :> obj
            | _ -> raise (ArgumentException())
            
        
[<Struct>]
type Payload =
    {
        area_code: string
        area_type: string
        area_name: string
        [<JsonField("date", Transform=typeof<DateTransform>)>]
        date: string
        metric: string
        value: float32 option
        priority: int
    }
    
[<Struct>]
type ChangeLogPayload =
    {
        id: string
        date: DateTime
        high_priority: bool
        tag: string
        heading: string
        body: string
    }
 
[<Struct>]   
type AnnouncementPayload =
    {
        date: DateTime
        body: string
    }
    
let private DBConnection =
    Sql.host (Environment.GetEnvironmentVariable "POSTGRES_HOST")
        |> Sql.database (Environment.GetEnvironmentVariable "POSTGRES_DATABASE")
        |> Sql.username (Environment.GetEnvironmentVariable "POSTGRES_USER")
        |> Sql.password (Environment.GetEnvironmentVariable "POSTGRES_PASSWORD")
        |> Sql.requireSslMode
        |> Sql.trustServerCertificate true
        |> Sql.formatConnectionString
    
type IDatabase<'T> =
    abstract member fetchFromDB: Async<string option>
    
[<AbstractClass>]
type DataBase<'T>(redis: Redis.Client, date: TimeStamp.Release) =
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
        member this.fetchFromDB =
            async {
                let! result =
                    this.preppedQuery
                    |> Sql.executeAsync
                        (fun read ->
                            {
                                area_type = read.string "area_type"
                                area_code = read.string "area_code"
                                area_name = read.string "area_name"
                                date = read.string "date"
                                metric = read.string "metric"
                                value = read.floatOrNone "value"
                                priority = read.int "priority"
                            }
                        )
                    |> Async.AwaitTask

                return! redis.SetAsync
                            this.key
                            result
                            this.cacheDuration
            }
            
    interface IDatabase<PostCodeDataPayload> with
        member this.fetchFromDB =
            async {
                let! result =
                    this.preppedQuery
                    |> Sql.executeAsync
                        (fun read ->
                            {
                                id = read.int "id"
                                area_type = read.string "area_type"
                                area_name = read.string "area_name"
                                postcode = read.string "postcode"
                                priority = read.int "priority"
                            }
                        )
                    |> Async.AwaitTask
                
                return! redis.SetHashAsync
                            this.key
                            this.keySuffix
                            result
            }

    interface IDatabase<ChangeLogPayload> with
        member this.fetchFromDB =
            async {
                let! result = 
                    this.preppedQuery
                    |> Sql.executeAsync
                        (fun read ->
                            {
                                id = read.string "id"
                                date = read.dateTime "date"
                                high_priority = read.bool "high_priority"
                                tag = read.string "tag"
                                heading = read.string "heading"
                                body = read.string "body"
                            }
                        )
                    |> Async.AwaitTask
            
                return! redis.SetAsync
                            this.key
                            result
                            this.cacheDuration
            }
            
    interface IDatabase<AnnouncementPayload> with
        member this.fetchFromDB =
            async {
                let! result = 
                    this.preppedQuery
                    |> Sql.executeAsync
                        (fun read ->
                            {
                                date = read.dateTime "date"
                                body = read.string "body"
                            }
                        )
                    |> Async.AwaitTask
            
                return! redis.SetAsync
                            this.key
                            result
                            this.cacheDuration
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
