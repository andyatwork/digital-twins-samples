using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Models;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace SampleFunctionsApp
{
    public class SampleDtHttpTrigger
    {
        const string adtAppId = "https://digitaltwins.azure.net";
        private static string adtInstanceUrl = $"https://{Environment.GetEnvironmentVariable("ADT_SERVICE_URL")}";
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("HttpTriggerCSharp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] 
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Function has been called.");
            DigitalTwinsClient client = null;
            
            try
            {
                // Authenticate on ADT APIs
                var credentials = new DefaultAzureCredential(true);
                client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credentials, new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"ADT service client connection created.");

                log.LogInformation($"Attempting to call ADT REST at {adtInstanceUrl}");
                var randomVersion = new Random().Next(0, Int32.MaxValue);
                var randomName = Guid.NewGuid().ToString();
                var createModels = new[] { @"{
                    ""@id"": ""dtmi:example:Floor;" + randomVersion.ToString() + @""",
                    ""@type"": ""Interface"",
                    ""displayName"": """ + randomName + @""",
                    ""@context"": ""dtmi:dtdl:context;2"",
                    ""contents"": [
                        {
                            ""@type"": ""Relationship"",
                            ""name"": ""contains"",
                            ""displayName"": ""contains"",
                            ""properties"": [
                                {
                                ""name"": ""RoomType"",
                                ""@type"":""Property"",
                                ""schema"": ""string""
                                }
                            ]
                        }
                    ]
                }" };

                var result = await client.CreateModelsAsync(createModels);
                return (ActionResult)new OkObjectResult($"Authenticated and authorized! Created model Id: {result.Value.First().Id}.");
            }
            catch (Exception e)
            {
                return (ActionResult)new OkObjectResult($"Error: {e.Message}");
            }
        }
    }
}