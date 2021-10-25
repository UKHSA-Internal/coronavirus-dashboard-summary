module coronavirus_dashboard_summary.Templates.ChangeLogBanners

open Giraffe.ViewEngine
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Constants

let private bannerItem (banner: DB.ChangeLogPayload) =
    li [ _class "change-log-banner" ] [
        div [ _class "govuk-body-s govuk-!-font-weight-bold govuk-!-margin-bottom-0" ] [
            strong [ _class "govuk-tag"; _style "background:white;color: #1d70b8 !important;margin:0 1rem 0 0;line-height:initial;" ] [
                encodedText banner.tag
            ]
            time [
                banner.date
                |> Formatter.toIsoString
                |> _datetime
            ] [
                banner.date.ToString Generic.DateFormat
                |> encodedText
            ]
            rawText " &mdash; "
            encodedText banner.heading
            a [
                _href $"/details/whats-new#{banner.id}"
                _class "govuk-link govuk-link--no-visited-state govuk-!-margin-left-1"
            ] [ encodedText "More" ]
        ]
    ]  

let Render (payload: DB.ChangeLogPayload list) =
    ul [ _class "change-logs govuk-!-padding-left-0" ] [
        yield! 
            payload
            |> List.map bannerItem
    ]
