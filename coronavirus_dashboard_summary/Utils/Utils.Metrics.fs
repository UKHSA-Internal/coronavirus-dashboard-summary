module coronavirus_dashboard_summary.Utils.Metrics

open System.Collections.Generic
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Utils.Constants

type GeneralPayload = Dictionary<string, DB.Payload>

let inline Processor (payload: GeneralPayload) =
    let inline metric (metric: string) =
        let inline attr (attribute: string) =
            match payload.TryGetValue metric with
            | true, m -> m.getter attribute
            | _       -> Generic.NotAvailable
        attr
    metric
