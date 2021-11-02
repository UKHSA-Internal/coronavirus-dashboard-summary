module coronavirus_dashboard_summary.Utils.Formatter

open System
open coronavirus_dashboard_summary.Utils.Constants

let toIsoString ( d: DateTime ) =
    d.ToString Generic.IsoTimeStampFormat

let toIsoDate ( d: DateTime ) =
    d.ToString Generic.IsoDateStampFormat

let inline toLongAreaType (areaType: string): string =
    match areaType with
    | AreaTypes.Overview  -> "United Kingdom"
    | AreaTypes.Nation
    | AreaTypes.Region    -> areaType
    | AreaTypes.UTLA      -> "Local authority (Upper tier)"
    | AreaTypes.LTLA      -> "Local authority (Lower tier)"
    | AreaTypes.MSOA      -> "MSOA"
    | AreaTypes.NHSRegion -> "Healthcare region"
    | AreaTypes.NHSTrust  -> "Healthcare trust"
    | _                   -> null

let inline pluralise (value: int32) single multi zero: string =
    match value with
    | v when v = 1 -> single
    | v when v > 1 -> multi
    | _            -> zero
    
let inline comparisonVerb (value: int32) up down same: string =
    match value with
    | v when v > 0 -> up
    | v when v < 0 -> down
    | _            -> same
