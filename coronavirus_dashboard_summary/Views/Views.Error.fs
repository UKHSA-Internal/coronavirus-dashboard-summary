module coronavirus_dashboard_summary.Views.Error

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Microsoft.ApplicationInsights
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Utils.TimeStamp

let Handler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let layout: Base.LayoutPayload =
                {
                    date     = ReleaseTimestamp()
                    banners  =
                        {
                            changeLogs    = None
                            announcements = rawText ""
                        }
                    title    = match ctx.Response.StatusCode with
                               | 404 -> "404 - Page not found"
                               | _   -> $"{ ctx.Response.StatusCode } - Server error"
                    postcode = null
                    error    = true
                }

            return!
                match ctx.Response.StatusCode with
                | 404 -> Error400.Render
                | _   -> Error500.Render
                |> layout.Render
                |> ctx.WriteHtmlViewAsync
        }
