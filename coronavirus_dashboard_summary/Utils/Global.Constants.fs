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
    let private Location = Environment.GetEnvironmentVariable "URL_LOCATION"
    let IsDev =
        match Environment.GetEnvironmentVariable "IS_DEV" with
        | "1" -> true
        | _ -> false
        
    let Environment = (Environment.GetEnvironmentVariable "API_ENV").ToUpper()
    
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
        | true -> ""
        | false -> $"https://{Location.TrimEnd('/')}"
