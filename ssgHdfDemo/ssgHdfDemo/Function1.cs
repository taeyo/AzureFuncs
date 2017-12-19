using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ssgHdfDemo
{
    public static class Function1
    {
        [FunctionName("FuncGetLastestBlobName")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("AzureStorage"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("delete");

            string s = string.Empty;
            s = await ListBlobsSegmentedInFlatListing(container, log);

            // if neeed to return json. use following sample
            //var obj = new { name = "Taeyo", hobby = "azure" };
            //var jsonToReturn = JsonConvert.SerializeObject(obj);

            //return new HttpResponseMessage(HttpStatusCode.OK)
            //{
            //    Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
            //};

            return s != null
            ? req.CreateResponse(HttpStatusCode.OK, s)
            : req.CreateResponse(HttpStatusCode.BadRequest, "There is no data");
        }
        
        public static async Task<string> ListBlobsSegmentedInFlatListing(CloudBlobContainer container, TraceWriter log)
        {
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            string lastWriteBlob = string.Empty;
            DateTimeOffset lastWrite = DateTimeOffset.MinValue;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            do
            {
                //This overload allows control of the page size. 
                // You can return all remaining results by passing null for the maxResults parameter, or by calling a different overload.
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.Metadata, 10, continuationToken, null, null);

                foreach (var blobItem in resultSegment.Results)
                {
                    DateTimeOffset? blobLastModified = ((CloudBlob)blobItem).Properties.LastModified ;

                    if (blobLastModified > lastWrite)
                    {
                        lastWrite = (DateTimeOffset)blobLastModified;
                        lastWriteBlob = ((CloudBlob)blobItem).Name;
                    }

                    log.Info(blobItem.Uri.ToString());
                }

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);

            return lastWriteBlob;
        }
    }
}
