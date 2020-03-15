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

    public class ExecStrategy : IExecutionStrategy
    {
        private readonly ExecutionStrategyDependencies _dependencies;

        public ExecStrategy(ExecutionStrategyDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public bool RetriesOnFailure { get; } 
        public TResult Execute<TState, TResult>(TState state, Func<DbContext, TState, TResult> operation, Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded)
        {
            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TState, TResult>(TState state, Func<DbContext, TState, CancellationToken, Task<TResult>> operation, Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
