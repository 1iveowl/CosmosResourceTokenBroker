using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using Newtonsoft.Json;

namespace CosmosResourceTokenClient
{
    internal class ResourceTokenBrokerClientService : IAsyncDisposable 
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _resourceTokenBrokerUri;

        internal ResourceTokenBrokerClientService(string resourceTokenBrokerUrl)
        {
            if (string.IsNullOrEmpty(resourceTokenBrokerUrl))
            {
                throw new NoNullAllowedException("Resource Token Broker Url construction parameter cannot be null");
            }
            
            _resourceTokenBrokerUri = new Uri(resourceTokenBrokerUrl);

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler);
        }

        internal async Task<IResourcePermissionResponse> GetResourceToken(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();

                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                _httpClient.DefaultRequestHeaders.Add("User-Agent", $"ResourceTokenBroker/0.9.0");
                _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                _httpClient.DefaultRequestHeaders.Add("Host", $"{_resourceTokenBrokerUri.Host}");
                _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", $"gzip, deflate, br");
                _httpClient.DefaultRequestHeaders.Add("Cache-Control", $"no-cache");
                _httpClient.DefaultRequestHeaders.Add("Connection", $"keep-alive");

                var response = await _httpClient.GetAsync(_resourceTokenBrokerUri);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content is null)
                    {
                        throw new CosmosClientException("Error acquiring resource token. Content missing is response from resource token broker.");
                    }

                    //var test = await response.Content.ReadAsStringAsync();

                    var serializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.Auto};


                    using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
                    using var jsonReader = new JsonTextReader(streamReader);

                    

                    var result = serializer.Deserialize<ResourcePermissionResponse>(jsonReader);

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new CosmosClientException("Error acquiring resource token.", ex);
            }

            throw new CosmosClientException("Error acquiring resource token.");
        }

        public async ValueTask DisposeAsync()
        {
            _httpClient?.Dispose();
            await Task.CompletedTask;
        }
    }
}
