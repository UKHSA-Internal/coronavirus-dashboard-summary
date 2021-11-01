module coronavirus_dashboard_summary.Templates.Error400

open Giraffe.ViewEngine

let Render =
    [
        div [ _class "text-width govuk-body" ] [
            h2 [ _class "govuk-heading-l" ] [
                "Page not found" |> encodedText
            ]
            p [] [
                "If you typed the web address, check it is correct." |> encodedText
            ]
            p [] [
                "If you pasted the web address, check you copied the entire address."
                |> encodedText
            ]
            p [] [
                "If the web address is correct or you selected a link or button, please "
                |> encodedText
                
                a [
                    _class "govuk-link govuk-link--no-visited-state"
                    _href "mailto:coronavirus-tracker@phe.gov.uk?Subject=Dashboard%20Error%20(404)"
                    _rel "noopener noreferrer"
                    _target "_blank"
                ] [
                    "contact us"
                    |> encodedText
                ]
                
                " to report the issue."
                |> encodedText
            ]
        ]
    ]
