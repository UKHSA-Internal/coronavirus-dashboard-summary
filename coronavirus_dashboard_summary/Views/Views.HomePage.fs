module coronavirus_dashboard_summary.Views.HomePageView 

open System
open Giraffe.ViewEngine
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open FSharp.Json
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Templates.Base
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Utils    
open coronavirus_dashboard_summary.Utils.TimeStamp

let HomePageMetrics =
    [|
        "newAdmissions"
        "newAdmissionsChange"
        "newAdmissionsChangePercentage"
        "newAdmissionsRollingSum"
        "newAdmissionsDirection"

        "newPeopleVaccinatedFirstDoseByPublishDate"
        "newPeopleVaccinatedSecondDoseByPublishDate"
        "cumPeopleVaccinatedFirstDoseByPublishDate"
        "cumPeopleVaccinatedSecondDoseByPublishDate"
        "cumVaccinationFirstDoseUptakeByPublishDatePercentage"
        "cumVaccinationSecondDoseUptakeByPublishDatePercentage"

        "newDeaths28DaysByPublishDate"
        "newDeaths28DaysByPublishDateChange"
        "newDeaths28DaysByPublishDateChangePercentage"
        "newDeaths28DaysByPublishDateRollingSum"
        "newDeaths28DaysByPublishDateDirection"
        "newDeaths28DaysByDeathDateRollingRate"

        "newCasesByPublishDate"
        "newCasesByPublishDateChange"
        "newCasesByPublishDateChangePercentage"
        "newCasesByPublishDateRollingSum"
        "newCasesByPublishDateDirection"
        "newCasesBySpecimenDateRollingRate"

        "newVirusTests"
        "newVirusTestsChange"
        "newVirusTestsChangePercentage"
        "newVirusTestsRollingSum"
        "newVirusTestsDirection"

    |]

let inline formatDate (d: DateTime) =
    d.ToString "d MMMM yyyy"
    
let inline subtractFormatDate (d: DateTime) (n: float) =
    d.AddDays n
    |> formatDate
    
let leadSection =
    [
        div [] [
            div [ _class "govuk-grid-row" ] [
                div [ _class "govuk-grid-column-full" ] [
                    h1 [ _class "govuk-heading-l govuk-!-margin-bottom-2 govuk-!-margin-top-2"
                         _data "nosnippet" "true" ] [ encodedText "UK Summary" ]
                    p [ _class "govuk-body-m govuk-!-margin-bottom-1 govuk-!-margin-top-3" ] [
                        encodedText
                            "The official UK government website for data and insights on coronavirus (COVID-19)."
                    ]
                ]
            ]
        ]
        div [] [
            p [ _class "govuk-body-m govuk-!-margin-bottom-3 govuk-!-margin-top-2" ] [
                encodedText "See the "
                a [ _class "govuk-link govuk-link--no-visited-state"; _href "/easy_read" ] [
                    encodedText "simple summary"
                ]
                encodedText " for the UK."
            ]
        ]
    ]
    
let inline changeLogBanners redis date =
    ChangeLogModel.ChangeLog.Data redis date
    |> Async.RunSynchronously
    |> ChangeLogBanners.Render
    
let inline index (date: Release) (redis: Redis.Client): XmlNode List =    
    let dbResp =
        redis.GetAllAsync [|$"area-{date.isoDate}-UK"|]
        |> Async.RunSynchronously
        |> Json.deserialize<DB.Payload list>
    
    [
        yield! leadSection
        article [] [
            ul [ _class "card-container" ] [
                yield!
                    MetaData.CardMetadata
                    |> Array.Parallel.map (fun metadata -> metadata.Card date dbResp null)
                    |> List.concat
            ]
        ]
    ]

let HomePageHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let date = ReleaseTimestamp()
            let redis = ctx.GetService<Redis.Client>()
            let changeLogs = changeLogBanners redis date
            
            let layout: LayoutPayload =
                {
                    date          = date
                    changeLogs    = changeLogs
                    title         = "UK Summary"
                    postcode      = null
                }

            return!
                index date redis
                |> layout.Render
                |> ctx.WriteHtmlViewAsync
        }
