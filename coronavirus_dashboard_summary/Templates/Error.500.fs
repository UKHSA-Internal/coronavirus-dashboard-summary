module coronavirus_dashboard_summary.Templates.Error500

open Giraffe.ViewEngine

let private content: string list =
    [
      "What were trying to do &ndash; e.g. Searching a postcode"
      "Your device &ndash; e.g. Mac, PC, iPhone"
      "Your operating system &ndash; e.g. Windows 10, MacOS 10.14, Android 11"
      "Your browser &ndash; e.g. Edge, Safari, Firefox, Chrome"
    ]

let Render =
    [
        div [ _class "text-width govuk-body" ] [
            h2 [ _class "govuk-heading-l" ] [
                "Sorry, there is a problem with the service." |> encodedText
            ]
            p [] [
                "Please try again later." |> encodedText
            ]
            p [] [
                "If the problem persists for more than one hour, "
                + "please contact us to report the issue. Including details "
                + "such as follows would help us diagnose and fix the problem as soon as possible: "
                |> encodedText
            ]
            ul [ _class "govuk-list govuk-list--bullet" ] [
                yield!
                    content
                    |> List.map (fun item -> li [] [ item |> rawText ])
            ]
        ]
    ]
