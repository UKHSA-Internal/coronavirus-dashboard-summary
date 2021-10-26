module coronavirus_dashboard_summary.Utils.Formatter

open System
open coronavirus_dashboard_summary.Utils.Constants

let toIsoString ( d: DateTime ) =
    d.ToString Generic.IsoTimeStampFormat

let toIsoDate ( d: DateTime ) =
    d.ToString Generic.IsoDateStampFormat

let toLongAreaType (areaType: string): string =
    match areaType with
    | AreaTypes.Overview -> "United Kingdom"
    | AreaTypes.Nation
    | AreaTypes.Region -> areaType
    | AreaTypes.UTLA -> "Local authority (Upper tier)"
    | AreaTypes.LTLA -> "Local authority (Lower tier)"
    | AreaTypes.MSOA -> "MSOA"
    | AreaTypes.NHSRegion -> "healthcare region"
    | AreaTypes.NHSTrust -> "healthcare trust"
    | _ -> null

let pluralise (value: int32) single multi zero: string =
    if value = 1 then single
    elif value > 1 then multi
    else zero
    
let comparisonVerb (value: int32) up down same: string =
    if value > 0 then up
    elif value < 0 then down
    else same
