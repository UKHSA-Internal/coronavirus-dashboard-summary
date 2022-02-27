module coronavirus_dashboard_summary.Utils.Metrics

open System.Collections.Generic
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Utils.Constants

    
/// <summary>
/// Constructs tuples from payload data, structured as (metric name, payload).
/// </summary>
/// <param name="item">Data payload</param>
let inline private constructPayloadTuple (item: DB.Payload): string * DB.Payload =
    (item.metric, item)


/// <summary>
/// Creates a dictionary of data from list of tuples.
/// </summary>
/// <param name="data">List of tuples as (metric name, payload)</param>
type GeneralPayload (data: DB.Payload list) =
    inherit Dictionary<string, DB.Payload>(data
                                           |> List.map constructPayloadTuple
                                           |> dict)
    
    /// <summary>
    /// Extracts metric and return a specific attribute from its payload.
    /// </summary>
    /// <param name="metric">Metric name</param>
    /// <param name="attribute">Attribute to extract from metric payload</param>
    member inline this.GetValue (metric: string) (attribute: string) =
        match this.TryGetValue metric with
        | true, m -> m.getter attribute
        | _       -> Generic.NotAvailable
