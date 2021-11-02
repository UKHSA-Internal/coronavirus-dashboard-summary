namespace coronavirus_dashboard_summary.Utils

open System
open System.Runtime.CompilerServices
open Azure.Storage.Blobs

module TimeStamp =
    [<Literal>]
    let private TimeStampBlob     = "info/latest_published"
    [<Literal>]
    let private IsoFormatTemplate = @"yyyy-MM-dd\THH:mm:ss.fffffff\Z"
    
    let private connStr           = Environment.GetEnvironmentVariable("DeploymentBlobStorage")
    let private containerClient   = BlobContainerClient(connStr, "pipeline")

    [<Struct; IsReadOnly>]
    type Release = {
        isoTimestamp:  string
        isoDate:       string
        timestamp:     DateTime
        partitionDate: string
        dateInt      : string
    }

    type Release with
        member inline this.AddDays (days: int) =
            this.timestamp.AddDays(float days)
            
        member inline this.SubtractDays (days: int) =
            this.timestamp.AddDays(float -days)
    
    
    let ReleaseTimestamp (): Release =
        let timestampBlobClient =
            containerClient.GetBlobClient(TimeStampBlob)

        let res =
            timestampBlobClient
                .DownloadContent()
                .Value
                .Content
                .ToString()
            
        let resObj =
            DateTime
                .ParseExact(res, IsoFormatTemplate, null)
        
        {
          isoTimestamp  = res
          isoDate       = res.Split("T").[0];
          timestamp     = resObj
          partitionDate = $"{resObj:yyyy_M_d}"
          dateInt       = $"{resObj:yyyyMMdd}" 
        }
