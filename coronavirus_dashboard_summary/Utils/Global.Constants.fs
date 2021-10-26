namespace coronavirus_dashboard_summary.Utils.Constants

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
    let private Location = System.Environment.GetEnvironmentVariable "URL_LOCATION"
    
    let IsDev =
        match System.Environment.GetEnvironmentVariable "IS_DEV" with
        | "1" -> true
        | _ -> false
        
    let Environment = (System.Environment.GetEnvironmentVariable "API_ENV").ToUpper()
    
    let InstrumentationKey = System.Environment.GetEnvironmentVariable "APPINSIGHTS_INSTRUMENTATIONKEY"
    
    
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
