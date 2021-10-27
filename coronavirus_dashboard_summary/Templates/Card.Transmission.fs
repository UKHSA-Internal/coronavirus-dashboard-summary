module coronavirus_dashboard_summary.Templates.Transmission

open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Templates.Components
open coronavirus_dashboard_summary.Models
open Giraffe.ViewEngine

let inline Card (metadata: MetaData.ContentMetadata) getter =
    let transmissionMin = getter "transmissionRateMin"
    let transmissionMax = getter "transmissionRateMax"

    let detailsLink =
        a [
            _class "govuk-link govuk-link--no-visited-state govuk-!-margin-top-2 bottom-aligned"
            _style "justify-self: start"
            _href (
                "/details/healthcare?areaType="
                + transmissionMin "area_type"
                + "&areaName="
                + transmissionMin "area_name"
                + "#card-reproduction_number_r_and_growth_rate"
            )
        ] [
            b[] [
                "All transmission data in "
                + transmissionMin "area_name"
                |> encodedText
            ]
        ]
    
    div [ _class "card-wide transmission govuk-!-margin-top-3" ] [
        div [ _class "content" ] [
            div [ _class "topic column govuk-!-margin-bottom-0" ] [
                div [] [
                    span [ _class "govuk-caption-m govuk-!-font-weight-regular" ] [
                        encodedText metadata.caption
                    ]
                ]                        
                h4 [ _class "govuk-heading-m title-mobile govuk-!-margin-bottom-0" ] [
                    $"{metadata.heading} in "
                    + transmissionMin "area_name"
                    |> encodedText
                ]
                p [ _class "grey govuk-!-font-size-14 govuk-!-margin-bottom-0 govuk-!-margin-top-0" ] [
                    transmissionMin "area_type"
                    |> areaTypeTag
                    
                    span [ _class "card-timestamp" ] [
                        "Latest data provided on "
                        |> encodedText
                        
                        time [ _style "white-space: nowrap"; _datetime (transmissionMin "date")  ] [
                            transmissionMin "formattedDate"
                            |> encodedText
                        ]
                    ]
                ]
                div [ _class "column link" ] [
                    div [] [ detailsLink ]
                ]
            ]
            ul [ _class "column govuk-list govuk-!-margin-bottom-0 center-aligned" ] [
                li [ _class "vertical-only" ] [
                    div [] [
                        span[ _class "govuk-!-margin-bottom-1" ] [ encodedText "R value, estimated range" ]
                        div [ _class "number-group" ] [
                            div [ _class "number-container" ] [
                                div [ _class "float tooltip" ] [
                                    div [ _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2" ] [
                                        span [ _class "govuk-link--no-visited-state number-link number" ] [
                                            transmissionMin "formattedValue"
                                            + " to "
                                            + transmissionMax "formattedValue"
                                            |> encodedText
                                        ]
                                        span [ _itemprop "disambiguatingDescription"; _class "tooltiptext govuk-!-font-size-16" ] [
                                            "Reported on "
                                            + transmissionMin "formattedDate"
                                            |> encodedText
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    div [ _class "govuk-!-padding-bottom-0 govuk-!-margin-top-2" ] [
                        details [ _class "govuk-details govuk-!-margin-bottom-0"; attr "data-module" "govuk-details" ] [
                            summary [ _class "govuk-details__summary govuk-!-margin-bottom-0" ] [
                                span [ _class "govuk-details__summary-text govuk-!-font-size-16" ] [
                                    encodedText "About the R value"
                                ]
                            ]
                            div [ _class "govuk-details__text govuk-body-s" ] [
                                p [] [
                                    "The average number of secondary infections "
                                    + "produced by a single infected person."
                                    |> encodedText
                                ]
                                ul [ _class "govuk-list govuk-list--bullet govuk-body-s" ] [
                                    li [] [
                                        encodedText "If R is greater than 1 the epidemic is growing."
                                    ]
                                    li [] [
                                        encodedText "If R is less than 1 the epidemic is shrinking."
                                    ]
                                    li [] [
                                        encodedText "If R is equal to 1 the total number of infections is stable."
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            div [ _class "mob-link additional-info bottom-aligned"; _style "width: 100%;" ] [
                hr [ _class "govuk-section-break govuk-section-break--visible"; _style "margin: 0 -20px;" ]
                p [ _class "govuk-!-margin-top-2 govuk-!-font-size-16 govuk-!-margin-bottom-0" ] [
                    detailsLink
                ]
            ]
        ]
    ]
