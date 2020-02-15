using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CosmosResourceToken.Core.Client;

namespace CosmosResourceTokenClient
{
    public class CosmosClient : ICosmosClient
    {



        public CosmosClient()
        {

        }

        public Task Create<T>(string id, T obj, DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        public Task<T> Read<T>(string id, DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        public Task Replace<T>(string id, T obj, DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        public Task Delete<T>(string id, DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetList<T>(DefaultPartitionKind defaultPartition)
        {
            throw new NotImplementedException();
        }
    }
}
