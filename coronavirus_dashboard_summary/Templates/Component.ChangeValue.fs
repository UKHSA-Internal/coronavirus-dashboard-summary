module coronavirus_dashboard_summary.Templates.ChangeValue

open System.Runtime.CompilerServices
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.Constants

[<IsReadOnly>]
type Payload =
    {
        change:              string
        changeFormatted:     string
        changeDirection:     string
        percentageFormatted: string
        startDate:           string
        endDate:             string
        caption:             string
    }
    
type Payload with
    member inline private this.colour: (string * string) =
         match this.caption with
         | "Testing" -> ("neutral", $"{Generic.UrlLocation}/public/assets/summary/images/arrow-up-grey.png")
         | _         -> match this.change.Substring(0, 1) with
                        | "" | "0" | "N/A" | "N" -> ("neutral", $"{Generic.UrlLocation}/public/assets/summary/images/arrow-up-grey.png")
                        | "-"                    -> ("good", $"{Generic.UrlLocation}/public/assets/summary/images/arrow-up-green.png")
                        | _                      -> ("bad", $"{Generic.UrlLocation}/public/assets/summary/images/arrow-up-red.png")
         
         
    member inline private this.directionText: string =
         match this.change.Substring(0, 1) with
         | "-" -> "a decrease"
         | _   -> "an increase"
    
    member inline this.numberBox: XmlNode List =
        match this.change with
        | "0" -> [ encodedText "No change" ] 
        | _   ->
            [
                span [ _class "govuk-visually-hidden" ] [
                    encodedText $"{ this.directionText } of "
                ]
                strong [ _class "govuk-!-margin-right-1" ] [
                    encodedText this.changeFormatted
                ]
                encodedText $"({this.percentageFormatted}%%)"
            ]
    
    member this.Render =
        [
            div [ _class "float tooltip" ] [
                div [ _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2" ] [
                    span [ _class "govuk-link--no-visited-state number-link-red" ] [
                        b [ _class $"govuk-tag number govuk-!-margin-top-1 change-box {fst this.colour}" ] [
                            img [
                                snd this.colour |> _src
                                _alt "Direction arrow"
                                _width "12px"
                                _ariaHidden "true"
                                _class "govuk-!-margin-right-1"
                                attr "loading" "lazy"
                                _style $"""transform: rotate({ this.changeDirection }deg)"""
                            ]
                            span [ _class "govuk-!-font-weight-regular" ] [
                                span [ _class "govuk-visually-hidden" ] [ encodedText "There has been " ]
                                yield! this.numberBox
                                span [ _class "govuk-visually-hidden" ] [ encodedText " compared to the previous 7 days." ]
                            ]
                        ]
                        span [ _class "tooltiptext govuk-!-font-size-16"; _itemprop "disambiguatingDescription" ] [
                            encodedText $"Change from previous 7 days ({this.startDate} - {this.endDate})"
                        ]
                    ]
                ]
            ]
        ]