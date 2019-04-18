using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ScaleCosmosDB.Shared;

namespace ScaleCosmosDB
{
    public static class CosmosDBNonBusinessHours
    {
        [FunctionName("CosmosDBNonBusinessHours")]
        public static void Run([TimerTrigger("0 0 18 * * *")]TimerInfo nonBusinessHourExecuteTimer, ILogger log)
        {
            //scale down at 6 PM everyday-- UTC 7AM
            try
            {
                log.LogInformation($"C# Timer trigger CosmosDB_NonBusinessHours function started at: {DateTime.Now}");
                //Scaling down Mercer Cache
                Utilities.SetCosmosDbScaling(Constants.Database.DataBaseName2, Constants.Collection.CollectionName, false,log);

                //Scaling down Loyalty Integration
                Utilities.SetCosmosDbScaling(Constants.Database.DataBaseName1, Constants.Collection.CollectionName, false,log);


                log.LogInformation($"C# Timer trigger CosmosDB_NonBusinessHours function completed at: {DateTime.Now}");
            }
            catch (Exception e)
            {
                log.LogError($"C# Timer trigger CosmosDB_NonBusinessHours function Error at: {DateTime.Now} with message : {e.Message}");
            }

        }
    }
}
