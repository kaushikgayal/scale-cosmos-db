using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ScaleCosmosDB.Shared;

namespace ScaleCosmosDB
{
    public static class CosmosDbBusinessHours
    {
        [FunctionName("CosmosDBBusinessHours")]
        public static void Run([TimerTrigger("0 0 9 * * *")]TimerInfo businessHourExecuteTimer, ILogger log)
        {
            //Scale up at 9 AM everyday -- UTC 10 PM
            try
            {
                log.LogInformation($"C# Timer trigger CosmosDB_BusinessHours function started at: {DateTime.Now}");
                //Scaling up Mercer Cache
                Utilities.SetCosmosDbScaling(Constants.Database.DataBaseName2, Constants.Collection.CollectionName, true,log);

                //Scaling up Loyalty Integration
                Utilities.SetCosmosDbScaling(Constants.Database.DataBaseName1, Constants.Collection.CollectionName, true,log);

                log.LogInformation($"C# Timer trigger CosmosDB_BusinessHours function completed at: {DateTime.Now}");
            }
            catch (Exception e)
            {
                log.LogError($"C# Timer trigger CosmosDB_BusinessHours function Error at: {DateTime.Now} with message : {e.Message}");
            }

        }
    }
}
