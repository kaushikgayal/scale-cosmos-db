using Microsoft.ApplicationInsights;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ScaleCosmosDB.Shared
{
    public static class Utilities
    {
        private static readonly string businessHourRUDataBaseName1 = ConfigurationManager.AppSettings["businessHourRUDatabase1"];

        private static readonly string nonBusinessHourRUDataBaseName1 = ConfigurationManager.AppSettings["nonBusinessHourRUDatabase1"];

        private static readonly string businessHourRUDataBaseName2 = ConfigurationManager.AppSettings["businessHourRUDatabase2"];

        private static readonly string nonBusinessHourRUDataBaseName2 = ConfigurationManager.AppSettings["nonBusinessHourRUDatabase2"];

        public static HttpResponseMessage SetCosmosDbScaling(string databaseName, string collectionName, bool scaleUp, ILogger log, int RU=0)
        {
            var telemetryClient = new TelemetryClient();
            string databaseURL = string.Empty;
            string databaseKey = string.Empty;
            try
            {
                //set configuration
                if (scaleUp)
                {
                    if (databaseName.Equals(Constants.Database.DataBaseName1))
                    {
                        telemetryClient.TrackEvent($"DataBaseName1-{collectionName}-ScaleUp");
                        databaseURL = ConfigurationManager.AppSettings["DataBaseName1EndPointAddress"];
                        databaseKey = ConfigurationManager.AppSettings["DataBaseName1Key"];
                        RU= RU == 0 ? Convert.ToInt32(businessHourRUDataBaseName1) : 0;
                        
                    }
                    else if (databaseName.Equals(Constants.Database.DataBaseName2))
                    {
                        telemetryClient.TrackEvent($"DataBaseName2-{collectionName}-ScaleUp");
                        databaseURL = ConfigurationManager.AppSettings["DataBaseName2EndPointAddress"];
                        databaseKey = ConfigurationManager.AppSettings["DataBaseName2Key"];
                        RU = RU == 0 ? Convert.ToInt32(businessHourRUDataBaseName2) : 0;
                    }

                }
                else
                {
                    //scale down
                    if (databaseName.Equals(Constants.Database.DataBaseName1))
                    {
                        telemetryClient.TrackEvent($"GuildGroup-{collectionName}-ScaleDown");
                        databaseURL = ConfigurationManager.AppSettings["DataBaseName1EndPointAddress"];
                        databaseKey = ConfigurationManager.AppSettings["DataBaseName1Key"];
                        RU = RU == 0 ? Convert.ToInt32(nonBusinessHourRUDataBaseName1) : -100;
                    }
                    else if (databaseName.Equals(Constants.Database.DataBaseName2))
                    {
                        telemetryClient.TrackEvent($"MercerData-{collectionName}-ScaleDown");
                        databaseURL = ConfigurationManager.AppSettings["DataBaseName2EndPointAddress"];
                        databaseKey = ConfigurationManager.AppSettings["DataBaseName2Key"];
                        RU = RU == 0 ? Convert.ToInt32(nonBusinessHourRUDataBaseName2) : -100;
                    }

                }

                //execute Scaling
                ScaleCosmosDB(databaseName,collectionName,databaseURL,databaseKey,log,RU);

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static async void ScaleCosmosDB(string databaseId, string collectionId, string databaseUri, string databaseKey, ILogger log, int newThroughput)
        {
            try
            {
                //1) initialize the document client
                using (DocumentClient client = new DocumentClient(new Uri(databaseUri), databaseKey))
                {
                    //2) get the database self link
                    string selfLink = client.CreateDocumentCollectionQuery(
                                        UriFactory.CreateDatabaseUri(databaseId))
                                            .Where(c => c.Id == collectionId)
                                            .AsEnumerable()
                                            .FirstOrDefault()
                                            .SelfLink;

                    //3) get the current offer for the collection
                    Offer offer = client.CreateOfferQuery()
                                    .Where(r => r.ResourceLink == selfLink)
                                    .AsEnumerable()
                                    .SingleOrDefault();

                    //4) get the current throughput from the offer
                    int throughputCurrent = (int)offer.GetPropertyValue<JObject>("content").GetValue("offerThroughput");
                    log.LogInformation($"Current provisioned throughput of Database:{databaseId}, Collection: {collectionId} is: {throughputCurrent.ToString()} RU.");

                    //5) get the RU increment from AppSettings and parse to an int
                    if (int.TryParse(ConfigurationManager.AppSettings["cosmosDB_RUIncrement"], out int RUIncrement))
                    {
                        //5.a) create the new offer with the throughput increment added to the current throughput
                        if(newThroughput == 0)
                        {
                            newThroughput = throughputCurrent + RUIncrement;
                        }
                        else if(newThroughput<0)
                        {
                            newThroughput=throughputCurrent - RUIncrement;
                        }

                        offer = new OfferV2(offer, newThroughput);

                        //5.b) persist the changes
                        await client.ReplaceOfferAsync(offer);
                        log.LogInformation($"New provisioned throughput of Database:{databaseId}, Collection: {collectionId} is: {newThroughput} RU.");

                    }
                    else
                    {
                        //5.c) if the throughputIncrement cannot be parsed return throughput not changed
                        throw new Exception("Throughput not changed!");
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                throw;
            }
        }
    }
}
