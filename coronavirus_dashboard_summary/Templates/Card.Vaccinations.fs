module coronavirus_dashboard_summary.Templates.Vaccinations

open System
open Giraffe.ViewEngine
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.Constants
open coronavirus_dashboard_summary.Models
open Giraffe.ViewEngine.Accessibility

[<Struct>]
type private NumberItem =
    {
        metric: string
        percentage: string
        label: string
        periodLabel: string
    }
    
let private contentMetadata =
    [
        ("first", [
            {
              metric = "newPeopleVaccinatedFirstDoseByPublishDate"
              percentage = null
              label = "First dose"
              periodLabel = "Daily"
            }
            {
              metric = "cumPeopleVaccinatedFirstDoseByPublishDate"
              percentage = "cumVaccinationFirstDoseUptakeByPublishDatePercentage"
              label = "First dose"
              periodLabel = "Total"
            }
        ])
        ("second", [
            {
              metric = "newPeopleVaccinatedSecondDoseByPublishDate"
              percentage = null
              label = "Second dose"
              periodLabel = "Daily"
            }
            {
              metric = "cumPeopleVaccinatedSecondDoseByPublishDate"
              percentage = "cumVaccinationSecondDoseUptakeByPublishDatePercentage"
              label = "Second dose"
              periodLabel = "Total"
            }
        ])
        ("booster", [
            {
              metric = "newPeopleVaccinatedThirdInjectionByPublishDate"
              percentage = null
              label = "Booster or third dose"
              periodLabel = "Daily"
            }
            {
              metric = "cumPeopleVaccinatedThirdInjectionByPublishDate"
              percentage = "cumVaccinationThirdInjectionUptakeByPublishDatePercentage"
              label = "Booster or third dose"
              periodLabel = "Total"
            }
        ])
    ] 

let private Observation content =
    li [ _itemprop "Observation"; _itemtype "https://schema.org/Observation"; _itemscope ]
        [ yield! content ]  


type private NumberItem with
    member this.Number areaType getter =
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
                                    encodedText
                                        ($"Number of people vaccinated ({this.label.ToLower()}) reported " +
                                        $"""on { getter this.metric "formattedDate" }""")
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ] |> Observation
        
    member this.Percentage areaType getter =
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
                        | _ -> Generic.NotAvailable
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
                                | _ -> Generic.NotAvailable
                                + "%"
                                |> encodedText
                                span [
                                    _class "tooltiptext govuk-!-font-size-16"
                                    _itemprop "disambiguatingDescription"
                                ] []
                                
                                span [ _class "tooltiptext govuk-!-font-size-16"; _itemprop "disambiguatingDescription"] [
                                    $"Percentage of population aged 12+ vaccinated ({ this.label.ToLower() }) reported on "
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
                    (match this.percentage.Contains "First" with
                    | true -> "square lightgreen"
                    | false -> "square darkgreen") |> _class
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
        let headingMetric = "newPeopleVaccinatedFirstDoseByPublishDate"
        let areaType = getter headingMetric "area_type"
        let areaNameConnector =
                match getter headingMetric "trimmedAreaName" with
                       | "" -> ""
                       | _ -> " in "
                       
        li [ _class "vaccinations"; _itemtype "https://schema.org/SpecialAnnouncement"; _itemprop "SpecialAnnouncement"; _itemscope ] [
            meta [ _itemprop "datePosted"; _content this.release.isoTimestamp ]
            meta [
                _itemprop "expires"
                (this.release.AddDays 1)
                |> Formatter.toIsoString
                |> _content
            ]
            meta [
                _itemprop "mainEntityOfPage"
                
                $"{ Generic.UrlLocation }/details/{ this.metadata.caption.ToLower() }"
                + match areaType with
                  | AreaTypes.Overview -> ""
                  | _ -> $"?areaType={ areaType }&areaName=" + getter headingMetric "area_name"
                |> _content
            ]
            meta [
                _itemprop "thumbnailUrl"
                $"{ Generic.UrlLocation }/downloads/homepage/{ release.isoDate }/vaccinations/{ areaType.ToLower() }/"
                + getter this.metadata.metric "area_code"
                + "_thumbnail.svg"
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
                        encodedText $"""Up to and including { getter this.metadata.metric "formattedDate" }"""
                    ]
                ]
                div [ _class "additional-info bottom-aligned" ] [
                    p [ _class "govuk-!-margin-bottom-0 govuk-!-margin-top-0 govuk-!-font-size-16" ] [
                        meta [
                            _itemprop "url"
                            
                            $"/details/{ this.metadata.caption.ToLower() }"
                            + "?areaType=" + (getter headingMetric "area_type").ToLower()
                            + "&areaName=" + (getter headingMetric "area_name").ToLower()
                            |> _content 
                        ]
                        a [
                            $"/details/{ this.metadata.caption.ToLower() }"
                            + "?areaType=" + (getter headingMetric "area_type").ToLower()
                            + "&areaName=" + (getter headingMetric "area_name").ToLower()
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
                    encodedText "Percentage of population aged 12+"
                ]
                figcaption [] [
                    ul [] [
                        yield!
                            contentMetadata
                            |> List.map (fun pair ->
                                            snd pair
                                            |> List.find (fun cnt -> String.IsNullOrEmpty cnt.percentage |> not)
                                            |> (fun cnt -> cnt.Percentage areaType getter))
                    ]
                ]
                div [ _class "graph"; _ariaHidden "true" ] [
                    a [
                        $"/details/{ this.metadata.caption.ToLower() }?areaType={ areaType.ToLower() }"
                        + "&areaName=" + (getter headingMetric "area_name").ToLower()
                        |> _href
                        
                        _class "govuk-link govuk-link--no-visited-state bottom-aligned"
                    ] [
                        img [
                            $"https://coronavirus.data.gov.uk/downloads/homepage/{ release.isoDate }/vaccinations/"
                            + getter this.metadata.metric "area_type"
                            + "/" + getter this.metadata.metric "area_code"
                            + "_thumbnail.svg"
                            |> _src
                            
                            "Chart displaying the percentage of population aged 12+ vaccinated in "
                            + getter this.metadata.metric "area_name"
                            |> _alt
                            
                            attr "loading" "lazy"
                        ]
                    ]
                ]
            ]
        ]

