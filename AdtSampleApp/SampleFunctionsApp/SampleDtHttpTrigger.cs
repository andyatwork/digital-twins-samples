using System;
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

                if (client != null)
                {
                    log.LogInformation($"Attempting to call ADT REST at {adtInstanceUrl}");
                    AsyncPageable<ModelData> result = client.GetModelsAsync();
                    await foreach (ModelData md in result)
                    {
                        log.LogInformation($"ModelId found: {md.Id}");
                    }
                    log.LogInformation("Call completed.");
                }
            }
            catch (Exception e)
            {
                return (ActionResult)new OkObjectResult($"Error: {e.Message}");
            }

            return (ActionResult)new OkObjectResult($"Wohoo, authenticated and authorized!");
        }
    }
}