namespace coronavirus_dashboard_summary.Utils.Constants

open System

module AreaTypes =
    [<Literal>]
    let Overview = "overview"
    
    [<Literal>]
    let Nation = "nation"
    
    [<Literal>]
    let Region = "region"
    
    [<Literal>]
    let UTLA = "utla"
    
    [<Literal>]
    let LTLA = "ltla"
    
    [<Literal>]
    let MSOA = "msoa"
    
    [<Literal>]
    let NHSRegion = "nhsRegion"
    
    [<Literal>]
    let NHSTrust = "nhsTrust"

    
module Generic =
    let private IsDev = Environment.GetEnvironmentVariable "IS_DEV"
    let private Location = Environment.GetEnvironmentVariable "URL_LOCATION"
    
    [<Literal>]
    let NotAvailable = "N/A"

    [<Literal>]
    let DateFormat = "d MMMM yyyy"
    
    [<Literal>]
    let IsoTimeStampFormat = "yyyy-MM-ddTHH:mm:ss.ssssssZ"
    
    [<Literal>]
    let IsoDateStampFormat = "yyyy-MM-dd"

    let UrlLocation =
        match IsDev with
        | "1" -> ""
        | _ -> $"https://{Location}"
