module coronavirus_dashboard_summary.Templates.Vaccinations

open System
open System.Runtime.CompilerServices
open Giraffe.ViewEngine
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.Constants
open coronavirus_dashboard_summary.Models
open Giraffe.ViewEngine.Accessibility

[<Struct; IsReadOnly>]
type private NumberItem =
    {
        metric:      string
        percentage:  string
        label:       string
        periodLabel: string
    }
    
let private contentMetadata =
    [
        ("second", [
            {
              metric      = "PeopleVaccinatedAutumn22ByVaccinationDate"
              percentage  = null
              label       = "autumn booster (aged 50+)"
              periodLabel = "Total"
            }
            {
              metric      = "cumVaccinationAutumn22UptakeByVaccinationDatePercentage"
              percentage  = "cumVaccinationAutumn22UptakeByVaccinationDatePercentage"
              label       = "Autumn booster"
              periodLabel = "Percentage"
            }
        ])
        
    ] 

let inline private Observation content =
    li [ _itemprop "Observation"; _itemtype "https://schema.org/Observation"; _itemscope ]
        [ yield! content ]  


type private NumberItem with
    member inline this.Number areaType getter =
        let metricAreaType = getter this.metric "area_type"
        
        [
            meta [ _itemprop "name"; _content $"{ this.periodLabel } vaccinations - { this.label.ToLower() }" ]
            meta [
                _itemprop "observationDate"
                _itemtype "https://schema.org/DateTime"
                getter this.metric "date" |> _content
            ]
            span [ _itemprop "Number"; _itemtype "https://schema.org/Number" ] [
                span [] [
                    meta [
                        _itemprop "QuantitativeValue"
                        _itemtype "https://schema.org/QuantitativeValue"
                        match areaType.Equals metricAreaType with
                        | true -> getter this.metric "value"
                        | _ -> Generic.NotAvailable
                        |> _content
                    ]
                ]
            ]
            strong [ _class "govuk-body-s float govuk-!-margin-bottom-0" ] [
                this.periodLabel |> encodedText
                " &ndash; " |> rawText
                this.label.ToLower() |> encodedText
            ]
            div [ _class "number-group" ] [
                div [ _class "number-container govuk-!-padding-right-4" ] [
                    div [ _class "float tooltip" ] [
                        div [
                            _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2"
                        ] [
                            span [ _class "govuk-link--no-visited-state number-link" ] [
                                match areaType.Equals metricAreaType with
                                | true -> getter this.metric "formattedValue"
                                | _ -> Generic.NotAvailable
                                |> encodedText
                                
                                span [
                                    _class "tooltiptext govuk-!-font-size-16"
                                    _itemprop "disambiguatingDescription"
                                ] [
                                    match this.periodLabel with
                                    | "Total" -> $"Total number of people vaccinated ({this.label.ToLower()}) up to and including "
                                    | _       -> $"Percentage of people vaccinated ({this.label.ToLower()}) up to and including "
                                    + getter this.metric "formattedDate"
                                    |> encodedText
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ] |> Observation
        
    member inline this.Percentage areaType getter =
        let metricAreaType = getter this.percentage "area_type"
        
        [
            meta [ _itemprop "name"; _content $"{this.periodLabel} vaccinations - {this.label.ToLower()}" ]
            meta [
                _itemprop "observationDate"
                _itemtype "https://schema.org/DateTime"
                getter this.metric "date" |> _content
            ]
            span [ _itemprop "Number"; _itemtype "https://schema.org/Number" ] [
                span [] [
                    meta [
                        _itemprop "QuantitativeValue"
                        _itemtype "https://schema.org/QuantitativeValue"
                        match areaType.Equals metricAreaType with
                        | true -> getter this.percentage "value"
                        | _    -> Generic.NotAvailable
                        |> _content
                    ]
                ]
            ]
            div [ _class "number-group" ] [
                div [ _class "number-container" ] [
                    div [ _class "float tooltip" ] [
                        div [ _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2" ] [
                            span [ _class "govuk-link--no-visited-state number-link" ] [
                                match areaType.Equals metricAreaType with
                                | true -> getter this.percentage "formattedValue"
                                | _    -> Generic.NotAvailable
                                + "%"
                                |> encodedText
                                span [
                                    _class "tooltiptext govuk-!-font-size-16"
                                    _itemprop "disambiguatingDescription"
                                ] []
                                
                                span [ _class "tooltiptext govuk-!-font-size-16"; _itemprop "disambiguatingDescription"] [
                                    $"Percentage of people aged 50+ ({ this.label.ToLower() }) reported on "
                                    + getter this.percentage "formattedDate"
                                    |> encodedText   
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            div [ _class "govuk-body-s float govuk-!-margin-top-1 govuk-!-margin-bottom-0" ] [
                span [
                    _ariaHidden "true"
                    match this.label with
                    | "First dose"            -> "square lightgreen"
                    | "Second dose"           -> "square darkgreen"
                    | "Booster or third dose" -> "square darkergreen"
                    | _                       -> ""
                    |> _class
                ] []
                strong [ _class "govuk-!-margin-left-1 govuk-!-font-weight-regular" ] [
                    encodedText this.label
                ]
            ]
        ] |> Observation


type Payload (metadata: MetaData.ContentMetadata, release: TimeStamp.Release) =
    
    member this.metadata
        with get() = metadata
        
    member this.release
        with get() = release
    
    member this.Render (getter: string -> string -> string) =
        let headingMetric = "newPeopleVaccinatedFirstDoseByPublishDateRollingSum"
        let areaType = getter headingMetric "area_type"
        let areaNameConnector =
                match getter headingMetric "trimmedAreaName" with
                       | "" -> ""
                       | _  -> " in "
                       
        li [ _class "vaccinations"; _itemtype "https://schema.org/SpecialAnnouncement"; _itemprop "SpecialAnnouncement"; _itemscope ] [
            meta [ _itemprop "datePosted"; _content this.release.isoTimestamp ]
            meta [ _itemprop "category"; _content "https://www.wikidata.org/wiki/Q81068910" ]
            meta [
                _itemprop "expires"
                (this.release.AddDays 1)
                |> Formatter.toIsoString
                |> _content
            ]
            span [
                _style "grid-column: -1; display: none"
                _itemscope
                _itemprop "spatialCoverage"
                
                "http://schema.org/"
                + match getter headingMetric "area_name" with
                  | "United Kingdom" -> "Country"
                  | _                -> "AdministrativeArea"
                |> _itemtype 
            ] [
                meta [ _itemprop "name"; getter headingMetric "area_name" |> _content ]
                meta [
                    _itemprop "sameAs"
                    
                    "https://en.wikipedia.org/wiki/"
                    + getter headingMetric "area_name"
                    |> _content
                ]
            ]
            meta [
                _itemprop "mainEntityOfPage"
                
                $"{ Generic.UrlLocation }/details/{ this.metadata.caption.ToLower() }"
                + match areaType with
                  | AreaTypes.Overview -> ""
                  | _                  -> $"?areaType={ areaType }&areaName=" + getter headingMetric "area_name"
                |> _content
            ]
            meta [
                _itemprop "thumbnailUrl"
                $"{ Generic.UrlLocation }/downloads/homepage/{ release.isoDate }/vaccinations/{ areaType.ToLower() }/"
                + getter this.metadata.metric "area_code"
                + "_50_plus_thumbnail.svg"
                |> _content
            ]
            meta [
                _itemprop "text"
                "Vaccines are given in doses to people aged 12 and over. In the 7 days to "
                + getter (snd contentMetadata.[0]).[0].metric "formattedDate"
                + ", "
                + getter (snd contentMetadata.[0]).[0].metric "formattedValue"
                + " people aged 12+ had been given a first dose, "
               
                + getter (snd contentMetadata.[0]).[1].metric "formattedDate"
                + ", "
                + getter (snd contentMetadata.[0]).[1].metric "formattedValue"
                + " ("
                + getter (snd contentMetadata.[0]).[1].percentage "formattedValue"
                + "%) of people aged 12+ have received a first dose, "
              
                |> _content
            ]
            div [ _class "topic" ] [
                strong [ _itemprop "name"; _class "govuk-caption-m govuk-!-font-weight-regular" ] [
                    encodedText this.metadata.caption
                ]
                h4 [ _class "govuk-heading-m title-mobile govuk-!-margin-bottom-0" ] [
                    $"{ this.metadata.heading } { areaNameConnector } " + getter headingMetric "trimmedAreaName"
                    |> encodedText
                ]
                p [ _class "grey govuk-!-font-size-14 govuk-!-margin-bottom-0 govuk-!-margin-top-1" ] [
                    getter headingMetric "area_type"
                    |> Components.areaTypeTag
                    span [ _class "card-timestamp" ] [
                        encodedText $"""Up to and including {(this.release.AddDays -1).Day} {(this.release.AddDays -1).ToString("MMMM")} {(this.release.AddDays -1).Year}"""
                    ]
                ]
                div [ _class "additional-info bottom-aligned" ] [
                    p [ _class "govuk-!-margin-bottom-0 govuk-!-margin-top-0 govuk-!-font-size-16" ] [
                        meta [
                            _itemprop "url"
                            
                            $"/details/{ this.metadata.caption.ToLower() }"
                            + "?areaType=" + (getter headingMetric "area_type")
                            + "&areaName=" + (getter headingMetric "area_name")
                            |> _content 
                        ]
                        a [
                            $"/details/{ this.metadata.caption.ToLower() }"
                            + "?areaType=" + (getter headingMetric "area_type")
                            + "&areaName=" + (getter headingMetric "area_name")
                            |> _href
                            
                            _class "govuk-link govuk-link--no-visited-state bottom-aligned govuk-!-margin-top-2 ext-link"
                        ] [
                            strong [] [
                                $"All { this.metadata.caption.ToLower() } data { areaNameConnector }"
                                + getter headingMetric "trimmedAreaName"
                                |> encodedText
                            ]
                        ]
                    ]
                ]
            ]
            
            yield!
                contentMetadata
                |> List.map (fun pair ->
                                ul [ fst pair |> _class ] [
                                    yield!
                                        snd pair
                                        |> List.map (fun cnt -> cnt.Number areaType getter)
                                ])
            
            figure [ _class "visaulisation"; _ariaLabelledBy "vaccination-vis-lab" ] [
                div [ _class "bottom-aligned main-caption govuk-!-font-size-16"; _id "vaccination-vis-lab" ] [
                    encodedText "Percentage of people aged 50+"
                ]
                figcaption [] [
                    
                ]
                div [ _class "graph"; _ariaHidden "true" ] [
                    a [
                        $"{ Generic.UrlLocation }/details/{ this.metadata.caption.ToLower() }"
                        + $"?areaType={ areaType.ToLower() }"
                        + "&areaName=" + (getter headingMetric "area_name")
                        |> _href
                        
                        _class "govuk-link govuk-link--no-visited-state bottom-aligned"
                    ] [
                        img [
                            $"{Generic.UrlLocation}/downloads/homepage/{ release.isoDate }/vaccinations/"
                            + getter this.metadata.metric "area_type"
                            + "/" + getter this.metadata.metric "area_code"
                            + "_50_plus_thumbnail.svg"
                            |> _src
                            
                            "Chart displaying the percentage of population aged 50+ given an autumn booster in "
                            + getter this.metadata.metric "area_name"
                            |> _alt
                        ]
                    ]
                ]
            ]
            div [ _class "mob-link additional-info" ] [
                // hr [ _class "govuk-section-break govuk-section-break--visible bottom-aligned"; _style "margin: 0 -20px;" ]
                p [ _class "bottom-aligned govuk-!-margin-top-2 govuk-!-font-size-16 govuk-!-margin-bottom-0" ] [
                    meta [
                        _itemprop "url"
                        
                        $"/details/{ this.metadata.caption.ToLower() }"
                        + "?areaType=" + (getter headingMetric "area_type")
                        + "&areaName=" + (getter headingMetric "area_name")
                        |> _content 
                    ]
                    a [
                        $"/details/{ this.metadata.caption.ToLower() }"
                        + "?areaType=" + (getter headingMetric "area_type")
                        + "&areaName=" + (getter headingMetric "area_name")
                        |> _href
                        
                        _class "govuk-link govuk-link--no-visited-state bottom-aligned govuk-!-margin-top-2"
                    ] [
                        strong [] [
                            $"All { this.metadata.caption.ToLower() } data { areaNameConnector }"
                            + getter headingMetric "trimmedAreaName"
                            |> encodedText
                        ]
                    ]
                ]
            ]
        ]

