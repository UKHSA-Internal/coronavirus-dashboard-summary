module coronavirus_dashboard_summary.Utils.Filters

open coronavirus_dashboard_summary.Models


let inline private groupByPriority (payload: DB.Payload): int = payload.priority


let inline GroupByPriorityAttribute (group: string * DB.Payload list) =
    snd group
    |> Seq.minBy groupByPriority


let inline GroupByMetric (payload: DB.Payload): string = payload.metric
