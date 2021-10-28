module coronavirus_dashboard_summary.Templates.PostCodeSearch

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Templates

let Render (postcode: string): XmlNode =
    section [ _class "mini-card"; _itemtype "https://schema.org/WebSite"; _itemscope ] [
        meta [ _itemprop "https://schema.org/WebSite"; _content "https://coronavirus.data.gov.uk/" ]
        meta [ _itemprop "url"; _content "https://coronavirus.data.gov.uk/" ]
        h3 [ _class "govuk-heading-m govuk-!-margin-bottom-2" ] [
            encodedText "Search by postcode"
            small [ _class "govuk-caption-m govuk-!-margin-top-1"; _itemprop "description" ] [
                encodedText "View data for your local area"
            ]
        ]
        form [
            _action "/search"
            _name "postcode-search"
            _method "GET"
            _itemtype "https://schema.org/SearchAction"
            _itemprop "potentialAction"
            _itemscope
            _id "regionform"
            _target "_self"
            _enctype "application/x-www-form-urlencoded"
        ] [
            meta [ _itemprop "target"; _content "https://coronavirus.data.gov.uk/search?postcode={postcode}" ]
            meta [ _itemprop "name"; _content "Search by postcode" ]
            div [
                "govuk-form-group govuk-!-margin-bottom-0"
                + match postcode with
                   | null -> null
                   | _    -> " govuk-form-group--error"
                |> _class
            ] [
                fieldset [ _class "govuk-fieldset"; attr "role" "group"; _ariaDescribedBy "postcode-hint" ] [
                    label [ _class "govuk-label govuk-!-font-weight-bold"; _for "postcode" ] [
                        encodedText "Enter a postcode"
                    ]
                    div [ _class "govuk-hint govuk-!-margin-bottom-3"; _itemprop "description" ] [
                        encodedText "For example SW1A 0AA"
                    ]
                    yield!
                        match postcode with
                        | null -> []
                        | _    ->
                            [
                              span [ _class "govuk-error-message" ] [
                                    Components.visuallyHidden "Error: "
                                    encodedText $"Invalid postcode '{ postcode }' "
                                    rawText "&mdash; enter a full and valid UK postcode."
                                ]
                            ]
                    p [ _class "govuk-!-margin-bottom-0" ] [
                        input [
                            _form "regionform"
                            "govuk-input govuk-input--width-10"
                            + match postcode with
                               | null -> null
                               | _    -> " govuk-input--error"
                            |> _class
                            _id "postcode"
                            _name "postcode"
                            _type "text"
                            _itemprop "query-input"
                            _pattern "[A-Za-z]{1,2}\d{1,2}[A-Za-z]?\s?\d{1,2}[A-Za-z]{1,2}\s{0,2}"
                            _style "text-transform: uppercase; max-width: 15ex;"
                            _maxlength "10"
                            _required
                        ]
                        label [ _for "submit-postcode"; _class "govuk-visually-hidden" ] [
                            encodedText "Submit"
                        ]
                        input [
                            _class "govuk-button govuk-!-margin-bottom-0"
                            _data "module" "govuk-button"
                            _id "submit-postcode"
                            _type "submit"
                            _value ""
                        ]
                    ]
                    p [ _class "govuk-!-margin-bottom-0" ] [
                        encodedText "Find a postcode on "
                        a [
                            _class "govuk-link govuk-link--no-visited-state"
                            _target "_blank"
                            _rel "noopener noreferrer"
                            _href "https://www.royalmail.com/find-a-postcode"
                        ] [
                            encodedText "Royal Mail's postcode finder"
                        ]
                    ]
                ]
            ]
        ]
    ]
