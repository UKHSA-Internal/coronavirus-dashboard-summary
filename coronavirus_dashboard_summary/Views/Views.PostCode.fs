module coronavirus_dashboard_summary.Views.PostCodeSearch

open System.Text.RegularExpressions
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks
open Giraffe
open Giraffe.ViewEngine
open FSharp.Json
open coronavirus_dashboard_summary.Models.ChangeLogModel
open coronavirus_dashboard_summary.Models.AnnouncementModel
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Templates.Base
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Models.MetaData
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Views.HomePageView
open coronavirus_dashboard_summary.Utils.TimeStamp

let PostCodeMetrics =
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
        "cumVaccinationFirstDoseUptakeByVaccinationDatePercentage"
        "cumVaccinationSecondDoseUptakeByVaccinationDatePercentage"
        "newDeaths28DaysByPublishDate"
        "newDeaths28DaysByPublishDateChange"
        "newDeaths28DaysByPublishDateChangePercentage"
        "newDeaths28DaysByPublishDateRollingSum"
        "newDeaths28DaysByPublishDateDirection"
        "newDeaths28DaysByDeathDateRollingRate"
        "newCasesBySpecimenDateRollingSum"
        "newCasesBySpecimenDateRollingRate"
        "newCasesBySpecimenDate"
        "newCasesByPublishDate"
        "newCasesByPublishDateChange"
        "newCasesByPublishDateChangePercentage"
        "newCasesByPublishDateRollingSum"
        "newCasesByPublishDateDirection"
        "newVirusTests"
        "newVirusTestsChange"
        "newVirusTestsChangePercentage"
        "newVirusTestsRollingSum"
        "newVirusTestsDirection"
        "transmissionRateMin"
        "transmissionRateMax"
    |]

let inline (|Regex|_|) pattern input =
    let found = Regex.Match(input, pattern)
    if found.Success then Some(List.tail [ for g in found.Groups -> g.Value ])
    else None

let inline validatePostcode (postcode: string): string =
    match postcode.ToUpper() with
    | Regex @"^\s*([A-Z]{1,2}\d{1,2}[A-Z]?\s?\d{1,2}[A-Z]{1,2})\s*$" [ validated ] ->
        validated.Replace(" ", "").ToUpper()
    | _ -> ""

let pageHeading =
    div [ _id "top" ] [
        div [ _class "sticky-header govuk-!-padding-top-3" ] [
            div [ _class "sticky-header govuk-grid-row govuk-!-margin-top-0" ] [
                div [ _class "govuk-grid-column-one-half" ] [
                    h1 [
                        "govuk-heading-m title govuk-!-margin-bottom-0 "
                        + "govuk-!-margin-top-0 panel-title-new"
                        |> _class
                    ] [
                        encodedText "Daily update"
                    ]
                ]
            ]
            div [ _class "govuk-grid-row govuk-!-margin-top-0" ] [
                div [ _class "govuk-grid-column-full" ] [
                    hr [
                        "govuk-heading-l govuk-!-margin-bottom-2 govuk-!-margin-top-0 "
                        + "govuk-!-margin-left-0 govuk-!-padding-top-0 govuk-!-padding-left-0"
                        |> _class
                    ]
                ]
            ]
        ]
    ]
    
let pageHeader (postcodeData: DB.PostCodeDataPayload list) =
    let smallestArea: DB.PostCodeDataPayload =
        postcodeData
        |> List.minBy (fun el -> el.priority)
    
    header [] [ 
        div [ _class "local-intro govuk-!-margin-left-0 govuk-!-margin-bottom-0" ] [
            h2 [
                "govuk-heading-l govuk-!-margin-bottom-2 govuk-!-margin-top-0 "
                + "govuk-!-margin-left-0 govuk-!-padding-top-0 govuk-!-padding-left-0"
                |> _class
            ] [
                encodedText smallestArea.postcode
                rawText " &mdash; "
                encodedText smallestArea.area_name
            ]
        ]
    ]
    
type PostCodeView(postcode: string, redis: Redis.Client) =
    
    let date = ReleaseTimestamp()
            
    let postcodeAreas =
        PostCode.Model(redis, date, validatePostcode postcode).PostCodeAreas
        |> Async.RunSynchronously
        
    member private _.changeLogBanners() =
        ChangeLog.Data redis date
        |> Async.RunSynchronously
        |> ChangeLogBanners.Render
            
    member private _.announcementBanners() =
        div[] []            
    member _.postCodeFound
        with get() = not <| List.isEmpty postcodeAreas
        
    member this.postcodeData =        
        postcodeAreas
        |> (fun postcodeData ->
                let layout: LayoutPayload =
                    {
                        date          = date
                        changeLogs    = this.changeLogBanners()
                        title         = ""
                        postcode      = match postcodeData.IsEmpty with
                                        | true -> postcode
                                        | _ -> null
                    }
                
                match postcodeData with
                | [] -> index date redis
                | _ -> postcodeData |> this.getContent
                |> layout.Render
        )
    
    member private _.getContent (postcodeData: DB.PostCodeDataPayload list) = 
        let dbResp =
            postcodeData
            |> List.map (fun item -> item.Key date PostCodeMetrics redis)
            |> List.toArray
            |> redis.GetAllAsync
            |> Async.RunSynchronously
            |> Json.deserialize<DB.Payload List>

        [
            pageHeading
            
            article [] [
                pageHeader postcodeData
                
                ul [ _class "govuk-list card-container" ] [
                    yield!
                        CardMetadata
                        |> Array.map (fun metadata -> metadata.Card date dbResp postcode)
                        |> List.concat
                ]
            ]
        ]

let PostCodePageHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let postcode =
                ctx.TryGetQueryStringValue "postcode"
                |> Option.defaultValue ""
                
            let redis = ctx.GetService<Redis.Client>()
                
            let view = PostCodeView(postcode, redis)
            
            if not view.postCodeFound then ctx.SetStatusCode 404

            return! ctx.WriteHtmlViewAsync view.postcodeData
        }
