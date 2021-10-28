module coronavirus_dashboard_summary.Templates.PostCodeHeading

open Giraffe.ViewEngine

let inline Render (postcode: string): XmlNode =
    div [ _id "top" ] [
        div [ _class "sticky-header govuk-!-padding-top-3" ] [
            div [ _class "sticky-header govuk-grid-row govuk-!-margin-top-0" ] [
                div [ _class "govuk-grid-column-one-half" ] [
                    h1 [ _class "govuk-heading-l govuk-!-margin-bottom-2 govuk-!-margin-top-0" ] [
                        encodedText $"Local summary for { postcode.ToUpper() }"
                    ]
                ]
            ]
        ]
    ]
