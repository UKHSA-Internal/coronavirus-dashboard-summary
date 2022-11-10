module coronavirus_dashboard_summary.Templates.Navigation

open System.Runtime.CompilerServices
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open coronavirus_dashboard_summary.Utils.Attrs
open coronavirus_dashboard_summary.Utils.Constants

[<IsReadOnly>]
type private NavigationItem = {
    label:   string
    uri:     string
    current: bool
}

let private primaryNavItems =
    [
        {
            label   = "Summary"
            uri     = "/"
            current = true
        }
        {
            label   = "Vaccinations" 
            uri     = "/details/vaccinations?areaType=nation&areaName=England" 
            current = false
        }
        {
            label   = "Testing" 
            uri     = "/details/testing?areaType=nation&areaName=England" 
            current = false
        }
        {
            label   = "Cases" 
            uri     = "/details/cases?areaType=nation&areaName=England" 
            current = false
        }
        {
            label   = "Healthcare" 
            uri     = "/details/healthcare?areaType=nation&areaName=England" 
            current = false
        }
        {
            label   = "Deaths" 
            uri     = "/details/deaths?areaType=nation&areaName=England" 
            current = false
        }
    ]

let private secondaryNavItems =
    [
        {
            label   = "Interactive maps"
            uri     = "/details/interactive-map"
            current = false
        }
        {
            label   = "Metrics documentation"
            uri     = "/metrics/category"
            current = false
        }
        {
            label   = "Download data"
            uri     = "/details/download"
            current = false
        }
        {
            label   = "What's new"
            uri     = "/details/whats-new"
            current = false
        }
        {
            label   = "Developer's guide"
            uri     = "/details/developers-guide"
            current = false
        }
        {
            label   = "About"
            uri     = "/about"
            current = false
        }
    ]        

type private NavigationItem with
    member inline private this.content parent =
        [
            a [
               _href $"{Generic.UrlLocation}{this.uri}" 
               match this.current with
               | true -> _ariaCurrent "page"; _class "govuk-link govuk-link--no-visited-state"
               | _    ->  _class "govuk-link govuk-link--no-visited-state"
            ] [ encodedText this.label ]
        ] |> parent
        
    member inline this.PrimaryNavItem =
        li [ match this.current with
             | true  -> "moj-side-navigation__item moj-side-navigation__item--active"
             | false -> "moj-side-navigation__item"
             |> _class
        ] |> this.content
        
    member inline this.SecondaryNavItem (mobile: bool) =
        match mobile with
        | false -> li [ _class "govuk-!-padding-bottom-1" ]
                   |> this.content
        | true  -> li [ _class "moj-side-navigation__item mobile" ]
                   |> this.content


let RenderMobile =
    div [ _class "mobile-nav"; _id "mobile-navigation"; _style "display: none;" ] [
        ul [ _class "govuk-list moj-side-navigation__list" ] [
            yield!
                primaryNavItems
                |> List.map (fun item -> item.PrimaryNavItem)
        ]
        hr [ _class "govuk-section-break govuk-section-break--m govuk-!-margin-top-3 govuk-!-margin-bottom-3 govuk-section-break--visible"]
        ul [ _class "govuk-list moj-side-navigation__list" ] [
            yield!
                secondaryNavItems
                |> List.map (fun item -> item.SecondaryNavItem true)
        ]
        hr [ _class "govuk-section-break govuk-section-break--l govuk-!-margin-top-3 govuk-!-margin-bottom-3 govuk-section-break--visible"]        
    ]

let RenderDesktop =
    nav [ _class "dashboard-menu" ] [
        div [ _class "moj-side-navigation govuk-!-padding-right-4 govuk-!-padding-top-2"
              attr "role" "navigation"
              _ariaLabel "Website navigation"
              _itemtype "http://schema.org/SiteNavigationElement"
              _itemscope ] [
            ul [ _class "moj-side-navigation__list" ] [
                yield!
                    primaryNavItems
                    |> List.map (fun item -> item.PrimaryNavItem)
            ]
            hr [ _class "govuk-section-break govuk-section-break--m govuk-!-margin-top-3 govuk-!-margin-bottom-3 govuk-section-break--visible"]
        ]
        div [ _class "tertiary-menu govuk-!-margin-left-3" ] [  
            ul [ _class "govuk-list govuk-!-font-size-14" ] [
                yield!
                    secondaryNavItems
                    |> List.map (fun item -> item.SecondaryNavItem false)
            ]
        ]
    ]
