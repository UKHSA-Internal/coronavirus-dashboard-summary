module coronavirus_dashboard_summary.Templates.Body

open Giraffe.ViewEngine

let inline Render (content: XmlNode list): XmlNode =
    article [] [                
        ul [ _class "govuk-list card-container" ] [
            yield! content
        ]
    ]
