using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CosmosResourceToken.Core;
using Microsoft.Azure.Cosmos;

namespace CosmosResourceTokenBroker
{
    public class BrokerService2 : IResourceTokenBrokerService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly string _collectionId;

        public BrokerService2(string endpointUrl, string key, string databaseId, string collectionId)
        {
            _cosmosClient = new CosmosClient(endpointUrl, key);
            _database = _cosmosClient.GetDatabase(databaseId);
            _collectionId = collectionId;
        }

        public Task<IPermissionToken> Get(string token, string userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();

        }

        //private async Task<IPermissionToken> CreateNewPermission(string userId, CancellationToken ct)
        //{
           
        //}


        public ValueTask DisposeAsync()
        {
            _cosmosClient?.Dispose();
            
            return new ValueTask();
        }
    }
}
