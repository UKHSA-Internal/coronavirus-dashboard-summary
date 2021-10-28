module coronavirus_dashboard_summary.Templates.PostCodeLead

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.Constants

let inline private Comparison postcode getter (release: TimeStamp.Release) =
    
    match getter "newCasesBySpecimenDateRollingRate" "value" with
    | Generic.NotAvailable -> div [] [
                   p [] [
                       "For smaller areas with fewer than 3 cases, we do not show data. "
                       + "This is to protect individuals' identities."
                       |> encodedText
                   ]
               ]
    | _ -> figure [ _ariaHidden "true"; _class "govuk-!-margin-0" ] [
               figcaption [
                   _class "govuk-!-font-weight-regular govuk-!-margin-bottom-1 govuk-!-margin-top-1"
               ] [
                   strong [ _class "govuk-body float govuk-!-margin-bottom-0" ] [
                       "Comparison to case rates in other "
                       + match getter "newCasesBySpecimenDateRollingRate" "area_type" with
                          | "msoa" -> "England"
                          | _      -> "UK"
                       + " areas"
                       |> encodedText
                   ]
               ]
               img [
                   _class "comparison-scale"
                   "https://coronavirus.data.gov.uk/public/assets/frontpage/scales/"
                   + $"{release.isoDate}/"
                   + getter "newCasesBySpecimenDateRollingRate" "area_type"
                   + "/"
                   + getter "newCasesBySpecimenDateRollingRate" "area_code"
                   + ".jpg"
                   |> _src
                   
                   attr "loading" "lazy"
                   
                   "Scale showing the comparison of "
                   + postcode
                   + " relative to national median."
                   |> _alt
               ] 
           ]
    
let inline private Vaccination getter =
    let baseMetric = "cumVaccinationFirstDoseUptakeByVaccinationDatePercentage"
    let secondaryMetric = "cumVaccinationSecondDoseUptakeByVaccinationDatePercentage"
    
    li [ _class "govuk-!-margin-top-2" ] [
        h3 [ _class "govuk-!-font-size-24 title-mobile govuk-!-margin-bottom-0 govuk-!-margin-top-0" ] [
            "People vaccinated in "
            + getter baseMetric "area_name"
            |> encodedText
            small [ _class "govuk-caption-m govuk-!-font-size-14 govuk-!-font-weight-regular govuk-!-margin-top-1" ] [
                getter baseMetric "area_type"
                |> Components.areaTypeTag
                span [ _class "govuk-!-margin-left-2" ] [
                    "Reported on "
                    + getter baseMetric "formattedDate"
                    |> encodedText
                ]
            ]
        ]
        ul [ _class "govuk-list numbers-container govuk-!-margin-top-2" ] [
            li [] [
                strong [ _class "govuk-body-s float govuk-!-margin-bottom-0" ] [ encodedText "1st dose" ]
                div [ _class "number-group" ] [
                    div [ _class "number-container" ] [
                        div [ _class "float tooltip" ] [
                            div [ _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2 govuk-!-font-weight-bold" ] [
                                span [ _class "govuk-link--no-visited-state number-link number" ] [
                                    getter baseMetric "formattedValue"
                                    + "%"
                                    |> encodedText   
                                ]
                                span [ _itemprop "disambiguatingDescription"; _class "tooltiptext govuk-!-font-size-16" ] [
                                    "Percentage of population aged 12+ vaccinated (first dose) reported on "
                                    + getter baseMetric "formattedDate"
                                    |> encodedText
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            li [] [
                strong [ _class "govuk-body-s float govuk-!-margin-bottom-0" ] [ encodedText "2nd dose" ]
                div [ _class "number-group" ] [
                    div [ _class "number-container" ] [
                        div [ _class "float tooltip" ] [
                            div [ _class "float govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2 govuk-!-font-weight-bold" ] [
                                span [ _class "govuk-link--no-visited-state number-link number" ] [
                                    getter secondaryMetric "formattedValue"
                                    + "%"
                                    |> encodedText   
                                ]
                                span [ _itemprop "disambiguatingDescription"; _class "tooltiptext govuk-!-font-size-16" ] [
                                    "Percentage of population aged 12+ vaccinated (second dose) reported on "
                                    + getter secondaryMetric "formattedDate"
                                    |> encodedText
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
    
    
let inline private SimpleSummary (postcode: string) (getter: string -> string -> string) =
    p [ _class "govuk-body-m govuk-!-margin-bottom-3" ] [
        encodedText "See the "
        a [
            _class "govuk-link govuk-link--no-visited-state"
            _href $"/easy_read?postcode={postcode}"
        ] [ encodedText "simple summary" ]
        " for "
        + getter "newCasesBySpecimenDateRollingSum" "area_name"
        + match getter "newCasesBySpecimenDateRollingSum" "area_type" with
           | AreaTypes.MSOA -> ", " + getter "newCasesByPublishDate" "area_name"
           | _ -> ""
        |> encodedText
    ]

let Render postcode getter (release: TimeStamp.Release) =
    let baseMetric = "newCasesBySpecimenDateRollingSum"
    let changePayload: WeeklyChange.Payload =
        {
            metric  = "newCasesBySpecimenDate"
            heading = "Total cases by date of specimen"
            caption = null
        }
    
    [
        li [ _class "simple-summary" ] [
            SimpleSummary postcode getter
        ]
        li [ _class "postcode-lead" ] [
            figure [ _class "graph govuk-!-margin-bottom-2 postcode-thumbnail"; _ariaHidden "true" ] [
                img [
                    "https://coronavirus.data.gov.uk/public/assets/frontpage/map_thumbnails/"
                    + getter baseMetric "area_code"
                    + ".svg"
                    |> _src
                    attr "loading" "lazy"
                    _alt "Thumbnail of the area on a map"
                ] 
            ]
            ul [ _class "postcode-lead-data" ] [
                li [] [
                    ul [ _class "govuk-list govuk-!-margin-top-2 lead-case-container" ] [
                        li [] [
                            h3 [ _class "govuk-!-font-size-24 title-mobile govuk-!-margin-0" ] [
                                "Cases in "
                                + getter baseMetric "area_name"
                                |> encodedText
                                small [ _class "govuk-caption-m govuk-!-font-size-14 govuk-!-font-weight-regular govuk-!-margin-top-1" ] [
                                    getter baseMetric "area_type"
                                    |> Components.areaTypeTag
                                    span [ _class "govuk-!-margin-left-2" ] [
                                        "By date of specimen in the seven days to "
                                        + getter baseMetric "formattedDate"
                                        |> encodedText
                                    ]
                                ]
                            ]
                            ul [ _class "govuk-list numbers-container govuk-!-margin-top-2" ] [
                                li [ _class "content-size" ] [
                                    strong [ _class "govuk-body-s float govuk-!-margin-bottom-0" ] [ encodedText "Last 7 days" ]
                                    div [ _class "number-group" ] [
                                        div [ _class "number-container" ] [
                                            div [ _class "float tooltip" ] [
                                                div [
                                                    _class "govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2 govuk-!-font-weight-bold"
                                                ] [
                                                    span [ _class "govuk-link--no-visited-state number-link number" ] [
                                                        getter "newCasesBySpecimenDateRollingSum" "formattedValue"
                                                        |> encodedText
                                                    ]
                                                    span [ _itemprop "disambiguatingDescription"; _class "tooltiptext govuk-!-font-size-16" ] [
                                                        "Total cases by specimen date in the 7 days to "
                                                        + getter "newCasesBySpecimenDateRollingSum" "formattedDate"
                                                        |> encodedText
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                    changePayload.Render getter
                                ]
                                li [] [
                                    strong [ _class "govuk-body-s float govuk-!-margin-bottom-0" ] [ encodedText "Cases per 100,000 people" ]
                                    div [ _class "number-group" ] [
                                        div [ _class "number-container" ] [
                                            div [ _class "float tooltip" ] [
                                                div [
                                                    _class "govuk-heading-m govuk-!-margin-bottom-0 govuk-!-padding-top-0 total-figure2 govuk-!-font-weight-bold"
                                                ] [
                                                    span [ _class "govuk-link--no-visited-state number-link number" ] [
                                                        getter "newCasesBySpecimenDateRollingRate" "formattedValue"
                                                        |> encodedText
                                                    ]
                                                    span [ _itemprop "disambiguatingDescription"; _class "tooltiptext govuk-!-font-size-16" ] [
                                                        "Rate of cases per 100,000 people by specimen date on "
                                                        + getter "newCasesBySpecimenDateRollingRate" "formattedDate"
                                                        |> encodedText
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                        li [ _class "figure" ] [ Comparison postcode getter release ]
                    ] 
                ]
                match getter "cumVaccinationFirstDoseUptakeByVaccinationDatePercentage" "area_type" with
                | AreaTypes.MSOA -> Vaccination getter
                | _              -> "" |> encodedText
            ]
        ]
        li [ _class "more-details" ] [
            div [] [
                h3 [ _class "govuk-!-margin-bottom-0 govuk-!-margin-top-5 govuk-heading-m" ] [
                    encodedText "Other data for your area"                      
                ]
                p [ _class "govuk-body-m govuk-!-margin-top-2 govuk-!-margin-bottom-1 gray-text" ] [
                    "We show the most local data available for each area."
                    |> encodedText
                ]
                p [ _class "govuk-body-m govuk-!-margin-top-0 gray-text" ] [
                    "Some data are only available for larger areas. For example, deaths are only available at "
                    + "local authority level."
                    |> encodedText
                ]
            ]
        ]
    ]
