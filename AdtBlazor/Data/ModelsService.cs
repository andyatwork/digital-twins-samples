using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.DigitalTwins.Core.Models;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace AdtBlazor.Data
{
    public class ModelsService
    {
        private readonly string adtInstanceUrl;
        private static HttpClient httpClient = new HttpClient();

        public ModelsService(IConfiguration configuration)
        {
            adtInstanceUrl = $"https://{configuration["ADT_SERVICE_URL"]}";
        }

        public async Task<ModelData[]> GetModelsAsync()
        {
            // Authenticate on ADT APIs
            var credentials = new DefaultAzureCredential(true);
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credentials, new DigitalTwinsClientOptions
            {
                Transport = new HttpClientTransport(httpClient)
            });

            AsyncPageable<ModelData> result = client.GetModelsAsync();

            var models = new List<ModelData>();

            await foreach(var model in result)
            {
                models.Add(model);
            }

            return models.ToArray();
        }
    }
}
