module coronavirus_dashboard_summary.Models.ChangeLogModel

open System
open Npgsql.FSharp
open coronavirus_dashboard_summary.Models.DB
open FSharp.Json

[<Literal>]
let private Query = "
SELECT DISTINCT cl.id::TEXT,
	  cl.date,
      cl.high_priority,
      t.tag,
      cl.heading,
      cl.body
FROM covid19.change_log AS cl
  LEFT JOIN covid19.tag                  AS t ON t.id = cl.type_id
  LEFT JOIN covid19.change_log_to_page   AS cltp ON cltp.log_id = cl.id
  LEFT JOIN covid19.page                 AS p ON p.id = cltp.page_id
WHERE cl.display_banner IS TRUE
  AND date = @date::DATE
  AND (
 	   cl.area ISNULL
    OR cl.area @> '{overview::^K.*$}'::VARCHAR[]
  )
ORDER BY date DESC;"

type ChangeLog(redis, date, telemetry) =
    inherit DataBase<ChangeLogPayload>(redis, date, telemetry)
    override this.keyPrefix     = "banner"
    override this.keySuffix     = "UK"
    override this.query         = Query
    override this.cacheDuration =
        {
            hours   = 0
            minutes = Random().Next(12, 18)
            seconds = Random().Next(0, 60)
        }
        
    override this.queryParams () =
        [
            "@date", Sql.timestamp this.date.timestamp
        ]
             
    static member inline Data redis date telemetry: Async<ChangeLogPayload list> =
        let fetcher = ChangeLog(redis, date, telemetry)
        
        async {
            let! result = redis.GetAsync fetcher.key (fetcher :> IDatabase<ChangeLogPayload>).fetchFromDB
                                      
            return match result with
                   | Some res -> Json.deserialize<ChangeLogPayload list>(res)
                   | _        -> []
        }