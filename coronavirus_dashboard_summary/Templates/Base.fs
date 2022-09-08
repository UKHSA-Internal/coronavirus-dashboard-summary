namespace coronavirus_dashboard_summary.Templates

open System
open System.Globalization
open System.Runtime.CompilerServices
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.TimeStamp
open coronavirus_dashboard_summary.Utils.Constants
open coronavirus_dashboard_summary.Templates.Footer
open coronavirus_dashboard_summary.Templates.Header

module Base =
    
    [<IsReadOnly>]
    type LayoutPayload =
        {
            date:     Release
            banners:  Banners.BannerPayload
            title:    string
            postcode: string
            error:    bool
        }
            
    let private ApiEnv = Environment.GetEnvironmentVariable "API_ENV"
    
    let private localZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London")
        
    [<Literal>]
    let private PageDescription =
        "Official Coronavirus (COVID-19) disease situation dashboard with latest data in the UK."
        
    let private envMetaTags =
        match ApiEnv with
        | "PRODUCTION" ->
            [
                meta [ _name "robot"; _content "all" ]
                meta [ _name "googlebot"; _content "all" ]
                meta [ _name "googlebot-news"; _content "all" ]
            ]
        | _ ->
            [
                meta [ _name "robot"; _content "noindex,nofollow" ]
                meta [ _name "googlebot"; _content "noindex,nofollow" ]
                meta [ _name "googlebot-news"; _content "noindex,nosnippet,nofollow" ]
                meta [ _name "AdsBot-Google"; _content "noindex,nofollow" ]
            ]
    
    let inline private TailCards ( content: XmlNode ): XmlNode =
        article [] [
            header [] [
                h2 [ _class "govuk-heading-l govuk-!-margin-top-4 govuk-!-margin-bottom-0" ] [
                    encodedText "What's the situation in your local area?"
                ]
            ]
            div [ _class "card-container double-row" ] [
                content
                
                section [
                    _class "mini-card map-view"
                    _itemtype "https://schema.org/WebContent"
                    _itemprop "WebContent"
                    _itemscope
                ] [
                    div [] [
                        span [
                            _itemprop "contentLocation"
                            _itemtype "https://schema.org/AdministrativeArea"
                            _itemscope
                        ] [
                            meta [ _itemprop "name"; _content "United Kingdom" ]
                            meta [ _itemprop "sameAs"; _content "https://en.wikipedia.org/wiki/United Kingdom" ]
                        ]
                        meta [ _itemprop "name"; _content "United Kingdom" ]
                        h3 [ _class "govuk-heading-m govuk-heading-m govuk-!-margin-bottom-3"; _itemprop "name" ] [
                            encodedText "UK interactive maps"
                        ]
                        div [ _class "govuk-body govuk-body-l govuk-!-margin-top-1 govuk-!-margin-bottom-3" ] [
                            p [ _class "govuk-!-margin-bottom-1" ] [
                                encodedText "Explore maps for:"
                            ]
                            ul [ _class "govuk-list govuk-list--bullet govuk-!-margin-top-0"; _itemscope ] [
                                li [ _itemprop "CreativeWork"; _itemtype "https://schema.org/CreativeWork"; _itemscope ] [
                                    span [ _itemprop "name" ] [ encodedText "cases" ]
                                    a [ _href "/details/interactive-map/cases"; _class "govuk-visually-hidden"; _itemprop "url" ] [
                                        encodedText "View map of cases"
                                    ]
                                ]
                                li [ _itemprop "CreativeWork"; _itemtype "https://schema.org/CreativeWork"; _itemscope ] [
                                    span [ _itemprop "name" ] [ encodedText "vaccinations" ]
                                    a [ _href "/details/interactive-map/vaccinations"; _class "govuk-visually-hidden"; _itemprop "url" ] [
                                        encodedText "View map of vaccinations"
                                    ]
                                ]
                            ]
                            a [ _href "/details/interactive-map/cases"; _class "govuk-button"; _rel "nofollow" ] [
                                encodedText "View maps"
                            ]  
                        ]
                    ]
                    div [ _style "text-align: center"; _ariaHidden "true"; _itemprop "thumbnailUrl" ] [
                        meta [ _itemprop "url"; _content "/public/assets/frontpage/images/map.png" ]
                        img [
                            _src "/public/assets/frontpage/images/map.png"
                            attr "loading" "lazy"
                            _ariaHidden "true"
                            _class "map-img"
                            "Map of the UK showing latest rate of new cases by "
                            + "specimen date for Upper Tier Local Authorities"
                            |> _alt
                        ]
                    ]
                ]
            ]
        ]
        
    type LayoutPayload with

        member this.Render (content: XmlNode list): XmlNode =
            // Windows only:
            // let localZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")
            let localDate = TimeZoneInfo.ConvertTime(this.date.timestamp, TimeZoneInfo.Utc, localZone)
            
            html [_lang "en"; _itemtype "https://schema.org/WebSite"; _itemscope] [
                head [] [
                    meta [_charset "utf-8"]
                    meta [attr "http-equiv" "x-ua-compatible"; _content "ie=edge"]
                    meta [ _name "viewport"; _content "width=device-width; initial-scale=1; shrink-to-fit=no" ]
                    meta [ _name "theme-color"; _content "#0b0c0c" ]
                    meta [ _name "twitter:card"; _content "summary_large_image" ]
                    meta [ _name "twitter:site"; _content "@UKHSA" ]
                    meta [ _property "og:type"; _content "website" ]
                    meta [ _property "og:locale"; _content "en_GB" ]
                    meta [ _name "instrumentation_key"; _content Generic.InstrumentationKey ]
                    baseTag [ _href Generic.UrlLocation ]
                    
                    yield! envMetaTags
                    
                    meta [
                        _property "og:image"
                        _content $"{Generic.UrlLocation}/downloads/og-images/og-summary_{this.date.dateInt}.png?{DateTime.UtcNow}"
                    ]
                    meta [
                        _property "twitter:image"
                        _content $"{Generic.UrlLocation}/downloads/og-images/og-summary_{this.date.dateInt}.png?{DateTime.UtcNow}"
                    ]
                    meta [
                        _itemprop "copyrightNotice"
                        _content "All content is available under the Open Government Licence v3.0; except where otherwise stated."
                    ]
                    
                    meta [ _name "description"; _itemprop "abstract"; _content PageDescription ]
                    meta [ _name "og:description"; _content PageDescription ]
                    meta [ _name "twitter:description"; _content PageDescription ]
                    
                    meta [ _name "logo"; _content $"{Generic.UrlLocation}/public/assets/summary/icon/favicon.png" ]
                    
                    link [ _rel "apple-touch-icon"; _href $"{Generic.UrlLocation}/public/assets/summary/icon/favicon.png" ]
                    link [ _rel "icon"; _href $"{Generic.UrlLocation}/public/assets/summary/icon/favicon.ico" ]
                    link [ _rel "manifest"; _href $"{Generic.UrlLocation}/manifest.json" ]
                    
                    meta [ _property "url"; _itemprop "url"; _content Generic.UrlLocation ]
                    meta [ _property "og:url"; _content Generic.UrlLocation ]

                    title [ _itemprop "name" ] [ encodedText $"{ this.title } | Coronavirus (COVID-19) in the UK" ]
                    meta [ _property "og:title"; _content $"{ this.title } | Coronavirus (COVID-19) in the UK" ]
                    meta [ _name "twitter:title"; _content $"{ this.title } | Coronavirus (COVID-19) in the UK" ]
                    
                    link [
                        _rel "preload"
                        _as "font"
                        _type "font/woff2"
                        _crossorigin
                        _href $"{Generic.UrlLocation}/public/assets/summary/fonts/light-94a07e06a1-v2.woff2"
                    ]
                    link [
                        _rel "preload"
                        _as "font"
                        _type "font/woff2"
                        _crossorigin
                        _href $"{Generic.UrlLocation}/public/assets/summary/fonts/bold-b542beb274-v2.woff2"
                    ]
                    link [
                        _rel "preload"
                        _as "style"
                        _type "text/css"
                        _crossorigin
                        _href $"{Generic.UrlLocation}/public/assets/summary/css/application.css"
                    ]
                                                          
                    link [
                        _rel  "stylesheet"
                        _type "text/css"
                        
                        match Generic.IsDev with
                        | false -> $"{Generic.UrlLocation}/public/assets/summary/css/application.css"
                        | true  -> "css/application.css"
                        |> _href 
                    ]
                    
                    script [ _type "application/javascript"; _async; _src "https://www.google-analytics.com/analytics.js"] []
                    script [ _type "application/javascript"; _async; _src $"{Generic.UrlLocation}/public/assets/summary/js/mscl.js"] []
                    script [ _type "application/javascript"; _src $"{Generic.UrlLocation}/public/assets/summary/js/gat.js"] []
                    script [ _type "application/javascript"; _async; _src $"{Generic.UrlLocation}/public/assets/summary/js/cookies.js"] []
                    
                    style [ _type "text/css" ] [
                        match this.banners.changeLogs with
                        | None   -> String.Empty
                        | Some _ -> ".banner {margin-top: .6rem !important;}"
                        |> rawText
                    ]
                ]
                body [ _class "govuk-template__body" ] [
                    yield! GovUKHeader
                    
                    Navigation.RenderMobile
                    
                    div [ _class "banner emergency" ] [
                        div [ _class "content" ] [
                            div [ _class "body" ] [
                                h2 [ _class "govuk-heading-m govuk-!-margin-bottom-1" ] [
                                    "Her Majesty Queen Elizabeth II" |> rawText
                                ]
                                p [ _class "govuk-body" ] [
                                    "21 April 1926 to 8 September 2022" |> rawText
                                ]
                                
                                p [ _class "govuk-body" ] [
                                    a [
                                        _class "govuk-body"
                                        _href "https://www.gov.uk/government/topical-events/her-majesty-queen-elizabeth-ii"
                                    ] [
                                        "Read about the arrangements following The Queen's death" |> rawText
                                    ]
                                ]
                            ]
                        ]
                    ]
                    
                    match this.banners.changeLogs with
                    | None   -> String.Empty |> rawText
                    | Some v -> v
                    
                    this.banners.announcements
                    
                    div [ _class "govuk-width-container" ] [
                        div [ _class "govuk-!-margin-top-5 govuk-!-margin-bottom-5"; attr "role" "region"; _ariaLabelledBy "last-update"] [
                            p [ _class "govuk-body-s"; _id "last-update" ] [
                                "Last updated on " |> encodedText
                                time [ _datetime this.date.isoTimestamp ] [
                                    localDate.ToString(
                                         @"dddd, d MMMM yyyy \a\t h:mmt\m",
                                         CultureInfo.CreateSpecificCulture("en-GB")
                                     ) |> encodedText
                                ]
                                span [ _itemprop "maintainer"; _itemscope ] [
                                    meta [ _itemprop "legalName"; _content "UK Health Security Agency" ]
                                    meta [ _itemprop "email"; _content "coronavirus-tracker@phe.gov.uk" ]
                                ]
                            ]
                        ]
                        div [ _class "govuk-main-wrapper" ] [
                            div [ _class "dashboard-container" ] [
                                Navigation.RenderDesktop
                                main [ _class "main"; _id "main-content" ] [
                                    yield! content
                                    
                                    match this.error with
                                    | false -> PostCodeSearch.Render this.postcode |> TailCards
                                    | true  -> rawText String.Empty
                                ]
                            ]
                        ]
                    ]
                    MainFooter
                    script [ _type "application/javascript"; _defer; _src $"{Generic.UrlLocation}/public/assets/summary/js/msai.js"] []
                    script [ _type "application/javascript"; _defer; _src $"{Generic.UrlLocation}/public/assets/summary/js/mobile_menu.js"] []
                ]
            ]        
