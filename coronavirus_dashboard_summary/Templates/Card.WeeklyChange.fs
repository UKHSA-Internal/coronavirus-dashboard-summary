module coronavirus_dashboard_summary.Templates.WeeklyChange

open Giraffe.ViewEngine
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Templates

type Payload =
    {
        metric:  string
        heading: string
        caption: string
    }

type Payload with
    member this.Render (getter: string -> string -> string) =
        let changeValuePayload: ChangeValue.Payload = 
            {
                change              = getter $"{this.metric}Change" "value"
                changeFormatted     = getter $"{this.metric}Change" "formattedValue"
                changeDirection     = getter $"{this.metric}Direction" "value"
                percentageFormatted = getter $"{this.metric}ChangePercentage" "formattedValue"
                startDate           = getter $"{this.metric}ChangePercentage" "13DaysAgoFormatted"
                endDate             = getter $"{this.metric}ChangePercentage" "7DaysAgoFormatted"
                caption             = this.caption
            }
        
        li [ _itemprop "Observation"; _itemtype "https://schema.org/Observation"; _itemscope ] [
            meta [ _itemprop "name"; _content $"{this.heading} - change in the last 7 days" ]            
            span [ _itemprop "observationDate" ] [
                meta [
                    _itemtype "https://schema.org/DateTime"
                    _itemprop "startDate"
                    changeValuePayload.endDate |> _content
                ]
                
                meta [
                    _itemtype "https://schema.org/DateTime"
                    _itemprop "endDate"
                    getter this.metric "date" |> _content
                ]
            ]
            
            span [ _itemprop "measuredValue" ] [
                span [
                    attr "itemtype" "https://schema.org/Number"
                    attr "itemprop" "Number"
                ] [
                    meta [ _itemprop "QuantitativeValue"
                           _itemtype "https://schema.org/QuantitativeValue"
                           getter $"{this.metric}Change" "value" |> _content ]
                ]
            ]

            yield! changeValuePayload.Render
        ]
