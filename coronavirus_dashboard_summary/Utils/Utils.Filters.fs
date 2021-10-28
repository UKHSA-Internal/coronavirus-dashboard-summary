module coronavirus_dashboard_summary.Utils.Filters

open coronavirus_dashboard_summary.Models

let ByPriorityAttribute (group: string * DB.Payload list) =
    snd group
    |> Seq.minBy (fun v -> v.priority)
