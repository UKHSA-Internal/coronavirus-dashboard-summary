module coronavirus_dashboard_summary.Models.PostCode

open Giraffe
open Giraffe.GoodRead
open coronavirus_dashboard_summary.Utils
open coronavirus_dashboard_summary.Models.DB
open FSharp.Json
open Npgsql.FSharp
    
type Model (redis: Redis.Client, date: TimeStamp.Release, postcode: string) =
    inherit DataBase<PostCodeDataPayload>(redis, date)

    override this.keySuffix = postcode
    
    override this.cacheDuration = 
        {
            hours = 48
            minutes = 0
            seconds = 0
        }

    override this.queryParams () =
        [
            "@postcode", Sql.string postcode
        ]
        
    override this.key = "area-postcode"
    
    override this.query = """
SELECT id::INT AS id, ref.area_type, area_name, postcode, priority::INT AS priority
FROM covid19.postcode_lookup
JOIN covid19.area_reference AS ref ON ref.id = area_id
JOIN covid19.area_priorities AS ap ON ap.area_type = ref.area_type
WHERE UPPER(REPLACE(postcode, ' ', '')) = @postcode;
"""
    
    member this.PostCodeAreas: Async<PostCodeDataPayload List> =
        async {
            let! result = Async.Choice
                            [
                                redis.GetHashAsync this.key this.keySuffix
                                (this :> IDatabase<PostCodeDataPayload>).fetchFromDB
                            ]
                            
            return match result with
                   | Some res -> Json.deserialize<PostCodeDataPayload List>(res)
                   | _ -> []
        }
