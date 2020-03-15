using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Console.EF.Cosmos.Client
{
    [Preserve(AllMembers = true)]
    public abstract class TokenClientDbContextBase : DbContext
    {
        private readonly string _cosmosUrl;
        private readonly string _resourceToken;
        private readonly string _dbName;
        private readonly string _partitionKey;

        protected TokenClientDbContextBase() { }

        protected TokenClientDbContextBase(
            string cosmosUrl,
            string resourceToken,
            string dbName,
            string partitionKey)
        {
            _cosmosUrl = cosmosUrl;
            _resourceToken = resourceToken;
            _dbName = dbName;
            _partitionKey = partitionKey;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseCosmos(
                    _cosmosUrl,
                    _resourceToken,
                    _dbName,
                    builder => builder.QueryRequestOption(
                        new QueryRequestOptions {PartitionKey = new PartitionKey(_partitionKey) }));

            base.OnConfiguring(optionsBuilder);
        }
    }
}
