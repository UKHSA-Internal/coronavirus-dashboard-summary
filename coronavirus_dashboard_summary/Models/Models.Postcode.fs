module coronavirus_dashboard_summary.Models.PostCode

open Giraffe
open Microsoft.ApplicationInsights
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Models.DB
open FSharp.Json
open Npgsql.FSharp
    
[<Literal>]
let private Query = "
SELECT id::INT AS id, ref.area_type, area_name, postcode, priority::INT AS priority
FROM covid19.postcode_lookup
  JOIN covid19.area_reference AS ref ON ref.id = area_id
  JOIN covid19.area_priorities AS ap ON ap.area_type = ref.area_type
WHERE UPPER(REPLACE(postcode, ' ', '')) = @postcode;
"

type Model (redis: Redis.Client, date: TimeStamp.Release, postcode: string, telemetry: TelemetryClient) =
    inherit DataBase<PostCodeDataPayload>(redis, date, telemetry)

    override this.keySuffix     = postcode
    override this.key           = "area-postcode"
    override this.query         = Query
    override this.cacheDuration = 
        {
            hours   = 48
            minutes = 0
            seconds = 0
        }

    override this.queryParams () =
        [
            "@postcode", Sql.string postcode
        ]
        
    member this.PostCodeAreas: Async<PostCodeDataPayload List> =
        async {
            let! result = redis.GetHashAsync this.key this.keySuffix (this :> IDatabase<PostCodeDataPayload>).fetchFromDB
                            
            return match result with
                   | Some res -> Json.deserialize<PostCodeDataPayload List>(res)
                   | _        -> []
        }
