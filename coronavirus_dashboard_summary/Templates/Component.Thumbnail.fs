module coronavirus_dashboard_summary.Templates.Thumbnail

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.Constants

[<Struct>]
type Payload =
    {
        isPostcode: bool
        caption: string
        metric: string
        metricData: string -> string
        date: TimeStamp.Release
    }

type Payload with 
    member this.Render =
        match this.isPostcode with
        | true -> "" |> encodedText
        | _ -> 
            a [
                $"{ Generic.UrlLocation }/details/{this.caption.ToLower()}?areaType="
                + (this.metricData "area_type").ToLower()
                + "&areaName=" + (this.metricData "area_name").ToLower()
                |> _href
                _ariaHidden "true"
                _class "govuk-link govuk-link--no-visited-state bottom-aligned"
                _style "text-decoration: none;"
            ] [
                meta [
                    _itemprop "thumbnailUrl"
                    $"{ Generic.UrlLocation }/downloads/homepage/"
                    + $"{this.date.isoDate}/thumbnail_{this.metric}.svg"
                    |> _content
                ]
                meta [
                    _itemprop "image"
                    $"{ Generic.UrlLocation }/downloads/homepage/"
                    + $"{this.date.isoDate}/thumbnail_{this.metric}.svg"
                    |> _content
                ]
                figure [ _class "graph mini-card-figure" ] [
                    img [
                        "https://coronavirus.data.gov.uk/downloads/homepage/"
                        + $"{this.date.isoDate}/thumbnail_{this.metric}.svg"
                        |> _src
                        
                        $"Graph of 7-day rolling average of {this.caption.ToLower()} "
                        + "over the last 3 months - click for more details"
                        |> _alt
                        
                        attr "loading" "lazy"
                    ]
                ]
            ]
