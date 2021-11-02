module coronavirus_dashboard_summary.Utils.JsonTransformers

open System
open FSharp.Json

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