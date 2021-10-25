namespace coronavirus_dashboard_summary.Templates

open System
open System.Text.RegularExpressions
open Giraffe.ViewEngine
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Utils.Metrics
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Templates

[<AutoOpen>]
module Card = 
    type MetaData.ContentMetadata with
        member private this.dailySection getter =

            li [ _class "data-metric2" ] [
                div [] [
                    meta [ _itemprop "name"; _content $"{this.heading} - daily" ]
                    meta [ _itemprop "observationDate"; getter this.metric "date" |> _content ]
                    span [ _itemprop "measuredValue" ] [
                        span [ attr "itemtype" "https://schema.org/Number"; attr "itemprop" "Number" ] [
                            meta [ _itemprop "QuantitativeValue"
                                   _itemtype "https://schema.org/QuantitativeValue"
                                   getter this.metric "value" |> _content ]
                        ]
                    ]
                ]
                strong [ _class "govuk-body-s float govuk-!-margin-bottom-0" ] [ encodedText "Daily" ]
                div [ _class "number-group" ] [
                    div [ _class "number-container" ] [
                        div [ _class "float tooltip" ] [
                            div [ _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2" ] [
                                span [ _class "govuk-link--no-visited-state number-link number" ] [
                                    getter this.metric "formattedValue" |> encodedText
                                ]
                                span [ _itemprop "disambiguatingDescription"; _class "tooltiptext govuk-!-font-size-16" ] [
                                    "Daily number of "
                                    + this.heading.ToLower()
                                    + " reported on "
                                    + getter this.metric "formattedDate"
                                    |> encodedText
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            
        member private this.last7Days getter =
            let changePayload: WeeklyChange.Payload =
                {
                    metric        = this.metric
                    heading       = this.heading
                }
                
            li [ _class "data-metric2" ] [
                strong [ _class "govuk-body-s float govuk-!-margin-top-3 govuk-!-margin-bottom-0" ] [
                    encodedText "Last 7 days"
                ]
                div [ _class "number-group" ] [
                    div [ _class "number-container govuk-!-padding-right-4" ] [
                        div [ _itemprop "Observation"; _itemtype "https://schema.org/Observation"; _itemscope ] [
                            meta [ _itemprop "name"; _content $"{this.heading} - sum of the last 7 days" ]
                            span [ _itemprop "observationDate" ] [
                                meta [ _itemtype "https://schema.org/DateTime"
                                       _itemprop "startDate"
                                       getter $"{this.metric}RollingSum" "7DaysAgo" |> _content ]
                                meta [ _itemtype "https://schema.org/DateTime"
                                       _itemprop "endDate"
                                       getter $"{this.metric}RollingSum" "date" |> _content ]
                            ]
                        ]
                        span [ _itemprop "measuredValue" ] [
                            span [
                                attr "itemtype" "https://schema.org/Number"
                                attr "itemprop" "Number"
                            ] [
                                meta [ _itemprop "QuantitativeValue"
                                       _itemtype "https://schema.org/QuantitativeValue"
                                       getter $"{this.metric}Change" "value" |> _content ]
                            ]
                        ]
                        div [ _class "float tooltip"; _itemprop "measuredValue" ] [
                            div [
                                attr "itemtype" "https://schema.org/Number"
                                attr "itemprop" "Number"
                                _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2"
                            ] [
                                span [
                                    _class "govuk-link--no-visited-state number-link"
                                    _itemprop "QuantitativeValue"
                                    _itemtype "https://schema.org/QuantitativeValue"
                                ] [
                                    getter $"{this.metric}RollingSum" "formattedValue" |> encodedText
                                ]
                                span [ _class "tooltiptext govuk-!-font-size-16"; _itemprop "disambiguatingDescription" ] [
                                    $"Total number of {this.heading.ToLower()} reported in the last 7 days ("
                                    + getter $"{this.metric}RollingSum" "6DaysAgoFormatted"
                                    + " - "
                                    + getter $"{this.metric}RollingSum" "formattedDate"
                                    + ")"
                                    |> encodedText
                                ]
                            ]
                        ]
                    ]
                ]
                
                changePayload.Render getter
            ]

        member private this.normalCard (date: TimeStamp.Release) (isPostcode: bool) getter =
            let metricData = getter this.metric
            let thumbnail: Thumbnail.Payload =
                {
                    isPostcode   = isPostcode
                    caption      = this.caption
                    metric       = this.metric
                    metricData   = metricData
                    date         = date
                }
                
            let areaNameConnector =
                match getter this.metric "trimmedAreaName" with
                       | "" -> ""
                       | _ -> " in " 

            li [ _class "mini-card"; _itemtype "https://schema.org/SpecialAnnouncement"; _itemprop "SpecialAnnouncement"; _itemscope ] [
                strong [
                    _class "govuk-caption-m govuk-!-font-weight-regular"
                    _itemprop "name"
                ] [ encodedText this.caption ]
                h4 [ _class "govuk-heading-m title-mobile govuk-!-margin-bottom-0" ] [
                    $"{ this.heading } { areaNameConnector } " + metricData "trimmedAreaName"
                    |> encodedText
                ]
                p [ _class "grey govuk-!-font-size-14 govuk-!-margin-bottom-0 govuk-!-margin-top-0" ] [
                    getter this.metric "area_type"
                    |> Components.areaTypeTag
                    
                    span [ _class "card-timestamp" ] [
                        "Latest data provided on "
                        |> encodedText
                        
                        time [ _style "white-space: nowrap"; _datetime (getter this.metric "date")  ] [
                            getter this.metric "formattedDate"
                            |> encodedText
                        ]
                    ]
                ]
                div [ _class "govuk-grid-row bottom-aligned" ] [
                    ul [ _class "govuk-grid-column-full" ] [
                        this.dailySection getter
                        this.last7Days getter
                    ]
                ]
                
                yield! RateDetail.Render this getter
                thumbnail.Render

                hr [ _class "govuk-section-break govuk-section-break--visible bottom-aligned" ]
                div [ _class "additional-info bottom-aligned" ] [
                    p [ _class "govuk-!-margin-bottom-0 govuk-!-margin-top-0 govuk-!-font-size-16" ] [
                        meta [
                            _itemprop "url"
                            
                            $"/details/{this.caption.ToLower()}"
                            + "?areaType=" + (metricData "area_type").ToLower()
                            + "&areaName=" + (metricData "area_name").ToLower()
                            |> _content 
                        ]
                        a [
                            _class "govuk-link govuk-link--no-visited-state"
                            
                            $"/details/{this.caption.ToLower()}"
                            + "?areaType=" + (metricData "area_type").ToLower()
                            + "&areaName=" + (metricData "area_name").ToLower()
                            |> _href
                        ] [
                            strong [] [
                                $"All { this.caption.ToLower() } data { areaNameConnector } "
                                + metricData "trimmedAreaName"
                                |> encodedText 
                            ]
                        ]
                    ]
                ]
            ]
              
        member this.Card (release: TimeStamp.Release) (payload: DB.Payload list) (postcode: string): XmlNode list =
            let getter = MetricValue(payload)
            let emptyPostcode = String.IsNullOrEmpty postcode
            
            match (emptyPostcode, this.postCodeOnly) with
            | (false, true) ->
                match String.IsNullOrEmpty this.caption with
                | false -> [ Transmission.Card this getter.metricValue ]
                | _ -> PostCodeLead.Render (postcode.ToUpper()) getter.smallestByMetric release
            | (true, false)
            | (false, false) ->
               match this.caption with
               | "Vaccinations" -> [ Vaccinations.Payload(this, release).Render getter.metricValue ]
               | _ -> [ this.normalCard release (not emptyPostcode) getter.metricValue ]
            | _ -> []
