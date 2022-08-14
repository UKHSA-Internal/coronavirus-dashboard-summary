module coronavirus_dashboard_summary.Templates.Footer

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils.Attrs

let MainFooter =
    footer [ _class "govuk-footer"; attr "role" "contentinfo" ] [
        div [ _class "govuk-width-container" ] [
            div [ _class "govuk-footer__meta" ] [
                div [ _class "govuk-footer__meta-item govuk-footer__meta-item--grow" ] [
                    h2 [ _class "govuk-visually-hidden" ] [ encodedText "Items" ]
                    ul [ _class "govuk-footer__inline-list" ] [
                        li [ _class "govuk-footer__inline-list-item" ] [
                            a [ _href "/details/announcements"; _class "govuk-footer__link" ] [
                                encodedText "Announcements"
                            ]
                        ]
                    ]
                    ul [ _class "govuk-footer__inline-list" ] [
                        li [ _class "govuk-footer__inline-list-item" ] [
                            a [ _href "/details/compliance"; _class "govuk-footer__link" ] [
                                encodedText "Compliance"
                            ]
                        ]
                        li [ _class "govuk-footer__inline-list-item" ] [
                            a [ _href "/details/accessibility"; _class "govuk-footer__link" ] [
                                encodedText "Accessibility"
                            ]
                        ]
                        li [ _class "govuk-footer__inline-list-item" ] [
                            a [ _href "/details/cookies"; _class "govuk-footer__link" ] [ encodedText "Cookies" ]
                        ]
                    ]
                    div [ _class "govuk-footer__meta-custom" ] [
                        p [
                            _class "govuk-footer__meta-custom"
                            _itemtype "https://schema.org/Organization"
                            _itemscope
                        ] [
                            meta [ _itemprop "name"; _content "UK Health Security Agency" ]
                            meta [
                                _itemprop "url"
                                _content "https://www.gov.uk/government/organisations/uk-health-security-agency"
                            ]
                            encodedText "For feedback email "
                            a [
                                _href "mailto:coronavirus-tracker@ukhsa.gov.uk?Subject=Dashboard%20feedback"
                                _class "govuk-footer__link"
                                _rel "noopener noreferrer"
                                _target "blank"
                                _itemprop "email"
                            ] [ encodedText "coronavirus-tracker@ukhsa.gov.uk" ]
                        ]
                        p [ _class "govuk-footer__meta-custom" ] [
                            encodedText "Developed by "
                            a [
                                _class "govuk-footer__link"
                                _href "https://www.gov.uk/government/organisations/uk-health-security-agency"
                                _target "blank"
                                _rel "noopener noreferrer"
                            ] [ encodedText "UK Health Security Agency" ]
                            encodedText "."
                        ]
                        p [ _class "govuk-footer__meta-custom" ] [
                            encodedText "This service is open source. See our repositories on "
                            a [
                                _class "govuk-footer__link"
                                _href "https://github.com/publichealthengland/coronavirus-dashboard#coronavirus-covid-19-in-the-uk"
                                _target "_blank"
                                _rel "noopener noreferrer"
                            ] [
                                rawText "GitHub &reg;"
                            ]
                        ]
                        tag "svg" [
                            _ariaHidden "true"
                            attr "focusable" "false"
                            _class "govuk-footer__licence-logo"
                            attr "xmlns" "http://www.w3.org/2000/svg"
                            attr "viewBox" "0 0 483.2 195.7"
                            _height "17"
                            _width "41"
                        ] [
                            voidTag "path" [
                                attr "fill" "currentColor"
                                attr "d" (
                                    "M421.5 142.8V.1l-50.7 32.3v161.1h112.4v-50.7zm-122.3-9.6A47.12 47.12 0 0 1 "
                                    + "221 97.8c0-26 21.1-47.1 47.1-47.1 16.7 0 31.4 8.7 39.7 21.8l42.7-27.2A97.63 "
                                    + "97.63 0 0 0 268.1 0c-36.5 0-68.3 20.1-85.1 49.7A98 98 0 0 0 97.8 0C43.9 0 0 "
                                    + "43.9 0 97.8s43.9 97.8 97.8 97.8c36.5 0 68.3-20.1 85.1-49.7a97.76 97.76 0 0 "
                                    + "0 149.6 25.4l19.4 22.2h3v-87.8h-80l24.3 27.5zM97.8 145c-26 "
                                    + "0-47.1-21.1-47.1-47.1s21.1-47.1 47.1-47.1 47.2 21 47.2 47S123.8 145 97.8 145"
                                    )
                            ]
                        ]
                        span [ _class "govuk-footer__licence-description" ] [
                            encodedText "All content is available under the "
                            a [
                                _class "govuk-footer__link"
                                _href "https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3"
                                _rel "license"
                            ] [
                                encodedText "Open Government Licence v3.0"
                            ]
                            encodedText ", except where otherwise stated."
                        ]
                    ]
                ]
                div [ _class "govuk-footer__meta-item" ] [
                    a [
                        _class "govuk-footer__link govuk-footer__copyright-logo"
                        "https://www.nationalarchives.gov.uk/information-management/"
                        + "re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/"
                        |> _href
                    ] [
                        rawText "&copy; Crown copyright"
                    ]
                ]
            ]
        ]
    ]
