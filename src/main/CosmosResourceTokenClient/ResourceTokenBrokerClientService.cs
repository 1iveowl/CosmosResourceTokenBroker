using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using Newtonsoft.Json;

namespace CosmosResourceTokenClient
{
    internal class ResourceTokenBrokerClientService
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

            _httpClient = new HttpClient();
        }

        internal async Task<IResourcePermissionResponse> GetResourceToken(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", accessToken);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Resource Token Broker Client"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip, deflate, br"));
            _httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");

            try
            {
                var response = await _httpClient.GetAsync(_resourceTokenBrokerUri);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content is null)
                    {
                        throw new CosmosClientException("Error acquiring resource token. Content missing is response from resource token broker.");
                    }

                    var serializer = new JsonSerializer();

                    using var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync());
                    using var jsonReader = new JsonTextReader(streamReader);

                    return serializer.Deserialize<ResourcePermissionResponse>(jsonReader);
                }
            }
            catch (Exception ex)
            {
                throw new CosmosClientException("Error acquiring resource token.", ex);
            }

            throw new CosmosClientException("Error acquiring resource token.");
        }
    }
}
