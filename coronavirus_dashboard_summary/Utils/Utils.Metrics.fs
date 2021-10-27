module coronavirus_dashboard_summary.Utils.Metrics

open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Utils.Constants

type MetricValue (payload: DB.Payload list) =
    member inline private _.getData (data: DB.Payload list) =
        fun metric ->
            let metricData =
                data
                |> List.tryFind (fun elm -> elm.metric.Equals metric)
                
            fun attribute ->
                match metricData with
                | Some v -> v |> fun elm -> elm.getter attribute
                | None -> Generic.NotAvailable
        
    member inline private _.filterPayload (group: string * DB.Payload List) =
        snd group
        |> Seq.minBy (fun v -> v.priority)
        
    member this.msoaMetricValue =
        let resp = payload
                   |> List.filter (fun elm -> elm.area_type.Equals AreaTypes.MSOA)
                   
        match resp.IsEmpty with
        | true -> payload |> this.getData
        | false -> resp |> this.getData
        
    member _.smallestByMetric metric attribute =
        let res =
            payload
            |> List.filter (fun elm -> elm.metric = metric)
        
        match List.isEmpty res with
        | true -> Generic.NotAvailable
        | _ -> res
               |> List.minBy (fun elm -> elm.priority)
               |> (fun elm -> elm.getter attribute)
              
        
    member this.metricValue  =
        payload
        |> List.filter (fun elm -> elm.area_type.Equals AreaTypes.MSOA |> not)
        |> List.groupBy (fun item -> item.metric)
        |> List.map this.filterPayload
        |> this.getData
