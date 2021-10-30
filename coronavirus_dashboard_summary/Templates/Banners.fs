module coronavirus_dashboard_summary.Templates.Banners

open System.Runtime.CompilerServices
open Giraffe.ViewEngine
open Microsoft.ApplicationInsights
open coronavirus_dashboard_summary.Models
open coronavirus_dashboard_summary.Templates
open coronavirus_dashboard_summary.Utils

[<IsReadOnly>]
type BannerPayload =
    {
        changeLogs:    XmlNode option
        announcements: XmlNode
    }

let inline Render (redis: Redis.Client) (release: TimeStamp.Release) (telemetry: TelemetryClient): BannerPayload =
    async {
        let! changeLog =
            ChangeLogModel.ChangeLog.Data redis release telemetry
            
        let! announcement =
            AnnouncementModel.Announcements.Data redis release telemetry
        
        return
            {
                changeLogs    = changeLog    |> ChangeLogBanners.Render
                announcements = announcement |> Announcement.Render
            }
    }
    |> Async.RunSynchronously

