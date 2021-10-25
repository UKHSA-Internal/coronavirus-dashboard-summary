module coronavirus_dashboard_summary.Models.ChangeLogModel

open System
open Npgsql.FSharp
open coronavirus_dashboard_summary.Models.DB
open FSharp.Json

type ChangeLog(redis, date) =
    inherit DataBase<ChangeLogPayload>(redis, date)
    override this.keyPrefix = "banner"
    override this.keySuffix = "UK"
    override this.cacheDuration =
        {
            hours = 0
            minutes = Random().Next(12, 18)
            seconds = Random().Next(0, 60)
        }
        
    override this.queryParams () =
        [
            "@date", Sql.timestamp this.date.timestamp
        ]
       
    override this.query =
        """SELECT DISTINCT cl.id::TEXT,
	          cl.date,
              cl.high_priority,
              t.tag ,
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
       ORDER BY date DESC;"""
      
    static member inline Data redis date: Async<ChangeLogPayload list> =
        let fetcher = ChangeLog(redis, date)
        
        async {
            let! result = Async.Choice
                              [
                                  redis.GetAsync fetcher.key
                                  (fetcher :> IDatabase<ChangeLogPayload>).fetchFromDB
                              ]
            
            return match result with
                   | Some res -> Json.deserialize<ChangeLogPayload list>(res)
                   | _ -> []
        }