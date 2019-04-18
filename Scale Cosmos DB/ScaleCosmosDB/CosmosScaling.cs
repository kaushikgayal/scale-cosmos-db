using System;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using ScaleCosmosDB.Model;
using ScaleCosmosDB.Shared;

namespace SampleFunction
{
    public static class CosmosScaling
    {
        [FunctionName("CosmosScaling")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]ScaleCosmos request, ILogger log)
        {
            try
            {
                Utilities.SetCosmosDbScaling(request.Database, request.CollectionName, request.ScaleUp, log, 10);
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
    }
}
