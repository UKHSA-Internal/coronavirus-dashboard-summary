module coronavirus_dashboard_summary.Templates.HomeHeading

open Giraffe.ViewEngine

let Render =
    [
        div [] [
            div [ _class "govuk-grid-row" ] [
                div [ _class "govuk-grid-column-full" ] [
                    h1 [ _class "govuk-heading-l govuk-!-margin-bottom-2 govuk-!-margin-top-2"
                         _data "nosnippet" "true" ] [ encodedText "England: Summary" ]
                    p [ _class "govuk-body-m govuk-!-margin-bottom-1 govuk-!-margin-top-3" ] [
                        encodedText
                            "The official UK government website for data and insights on coronavirus (COVID-19)."
                    ]
                ]
            ]
        ]
        div [] [
            p [ _class "govuk-body-m govuk-!-margin-bottom-3 govuk-!-margin-top-2" ] [
                encodedText "See the "
                a [ _class "govuk-link govuk-link--no-visited-state"; _href "/easy_read" ] [
                    encodedText "simple summary"
                ]
                encodedText " for England."
            ]
        ]
    ]
