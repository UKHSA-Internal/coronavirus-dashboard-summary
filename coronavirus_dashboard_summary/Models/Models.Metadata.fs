module coronavirus_dashboard_summary.Models.MetaData

[<Struct>]
type ContentMetadata =
    {
        metric:              string
        rate:                string
        caption:             string
        heading:             string
        postcodeDestination: string
        rateOffset:          bool
        postCodeOnly:        bool
        description:         string
    }

let CardMetadata: ContentMetadata[] = [|
    {
        metric              = "newCasesBySpecimenDate"
        rate                = "newCasesBySpecimenDateRollingRate"
        caption             = null
        heading             = null
        postcodeDestination = null
        rateOffset          = true
        postCodeOnly        = true
        description         = null
    }
    {
        metric              = "newPeopleVaccinatedFirstDoseByPublishDate"
        rate                = null
        caption             = "Vaccinations"
        heading             = "People vaccinated"
        postcodeDestination = "ltla"
        rateOffset          = true
        postCodeOnly        = false
        description         = null
    }
    {
        metric              = "newCasesByPublishDate"
        rate                = "newCasesBySpecimenDateRollingRate"
        caption             = "Cases"
        heading             = "People tested positive"
        postcodeDestination = "ltla"
        rateOffset          = true
        postCodeOnly        = false
        description         = "by date of specimen as of"

    }
    {
        metric              = "newDeaths28DaysByPublishDate"
        rate                = "newDeaths28DaysByDeathDateRollingRate"
        caption             = "Deaths"
        heading             = "Deaths within 28 days of positive test"
        postcodeDestination = "ltla"
        rateOffset          = true
        postCodeOnly        = false
        description         = "by date of death as of"
    }
    {
        metric              = "newAdmissions"
        rate                = null
        caption             = "Healthcare"
        heading             = "Patients admitted"
        postcodeDestination = "nhsTrust"
        rateOffset          = false
        postCodeOnly        = false
        description         = null
    }
    {
        metric               = "newVirusTests"
        rate                = null
        caption             = "Testing"
        heading             = "Virus tests conducted"
        postcodeDestination = "nation"
        rateOffset          = false
        postCodeOnly        = false
        description         = null
    }
    {
        metric              = "transmissionRateMin"
        rate                = null
        caption             = "Transmission"
        heading             = "R value"
        postcodeDestination = null
        rateOffset          = false
        postCodeOnly        = true
        description         = null
    }
|]
