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
    
let inline pluralise (value: string) single multi zero: string =
    match Int32.TryParse value with
    | true, v when v = 1 -> single
    | true, v when v > 1 -> multi
    | true, _            -> zero
    | _                  -> multi

let inline comparisonVerb (value: string) up down same: string =
    match Int32.TryParse value with
    | true, v when v > 0 -> up
    | true, v when v < 0 -> down
    | true, _            -> same
    | _                  -> String.Empty
