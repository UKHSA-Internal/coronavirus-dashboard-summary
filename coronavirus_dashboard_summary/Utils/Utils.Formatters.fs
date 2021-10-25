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
