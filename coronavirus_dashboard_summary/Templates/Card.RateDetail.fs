module coronavirus_dashboard_summary.Templates.RateDetail

open System
open coronavirus_dashboard_summary.Models
open Giraffe.ViewEngine

let inline Render (metadata: MetaData.ContentMetadata) (getter: string -> string -> string): XmlNode List =
    match String.IsNullOrEmpty metadata.rate with
    | true -> []
    | _ -> 
        [
            details [
                _class "govuk-details govuk-!-margin-top-1 govuk-!-margin-bottom-1"
                _data "module" "govuk-details"
            ] [
                summary [ _class "govuk-details__summary" ] [
                    span [ _class "govuk-details__summary-text body-small" ] [
                        encodedText "Rate per 100,000 people:"
                        rawText "&nbsp;"
                        strong [] [
                            getter metadata.rate "formattedValue"
                            |> encodedText
                        ]
                    ]
                ]
                div [ _class "govuk-details__text" ] [
                    span [ _class "body-small" ] [
                        encodedText "7-day rolling rate"
                        rawText "&nbsp;"
                        encodedText metadata.description
                        " "
                        + getter metadata.rate "formattedDate"
                        |> encodedText
                    ]
                ]
            ]
        ]
    
