namespace coronavirus_dashboard_summary.Utils

open System
open System.Runtime.CompilerServices
open Azure.Storage.Blobs

module TimeStamp =
    let private connStr = Environment.GetEnvironmentVariable("DeploymentBlobStorage")
    let private containerClient = BlobContainerClient(connStr, "pipeline")

    [<Struct; IsReadOnly>]
    type Release = {
        isoTimestamp:  string
        isoDate:       string
        timestamp:     DateTime
        partitionDate: string
    }

    type Release with
        member inline this.AddDays (days: int) =
            this.timestamp.AddDays(float days)
            
        member inline this.SubtractDays (days: int) =
            this.timestamp.AddDays(float -days)
    
    
    let ReleaseTimestamp (): Release =
        let timestampBlobClient =
            containerClient.GetBlobClient("info/latest_published")

        let res =
            timestampBlobClient
                .DownloadContent()
                .Value
                .Content
                .ToString()
            
        let resObj =
            DateTime.ParseExact(res, @"yyyy-MM-dd\THH:mm:ss.fffffff\Z", null)
        
        {
          isoTimestamp  = res
          isoDate       = res.Split("T").[0];
          timestamp     = resObj
          partitionDate = $"{resObj:yyyy_M_d}"
        }
