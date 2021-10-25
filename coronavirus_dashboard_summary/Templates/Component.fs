module coronavirus_dashboard_summary.Templates.Components

open Giraffe.ViewEngine
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Constants

let visuallyHidden (value: string) =
    span [ _class "govuk-visually-hidden" ] [
        value
        |> encodedText
    ]

let areaTypeTag (areaType: string) =
    match areaType with
    | AreaTypes.Overview -> encodedText ""
    | _ ->
        strong[ _class "areatype-tag" ] [
            visuallyHidden "Available at "
            Formatter.toLongAreaType areaType
            |> encodedText
            visuallyHidden " level."
        ]
