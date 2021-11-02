module coronavirus_dashboard_summary.Templates.Announcement

open System
open Giraffe.ViewEngine
open FSharp.Formatting.Markdown
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Constants

let inline private bannerItem (banner: DB.AnnouncementPayload) =
    li [ _class "content" ] [
        strong [ _class "timestamp" ] [
            time [
                banner.date
                |> Formatter.toIsoString
                |> _datetime
            ] [
                banner.date.ToString Generic.DateFormat
                |> encodedText
            ]
        ]
        div [ _class "body" ] [
            banner.body
            |> Markdown.ToHtml
            |> rawText
        ]
    ]  

let inline Render (payload: DB.AnnouncementPayload list): XmlNode =
    match payload.IsEmpty with
    | true  -> String.Empty |> rawText
    | false -> ul [ _class "banner" ] [
                   yield! 
                       payload
                       |> List.map bannerItem
               ]
