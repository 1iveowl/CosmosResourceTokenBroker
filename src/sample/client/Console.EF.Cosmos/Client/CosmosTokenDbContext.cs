using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;
using Microsoft.EntityFrameworkCore;

namespace Console.EF.Cosmos.Client
{
    [Preserve(AllMembers = true)]
    public class CosmosTokenDbContext<T> : BrokerClientHandler where T : DbContext, new()
    {
        private DbContext _context;
        
        public CosmosTokenDbContext(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey resourceTokenCache = null) : base(authService, resourceTokenBrokerUrl, resourceTokenCache)
        {

        }

        public async Task<T> GetDbContextAsync(PermissionModeKind permissionMode, CancellationToken ct = default) =>
            await ExecuteCommandWrapper(resourcePermissionResponse =>
            {
                var resourceToken = resourcePermissionResponse?.ResourcePermissions?
                    .FirstOrDefault(r => r.PermissionMode == permissionMode)?.ResourceToken;
                
                var dbName = resourcePermissionResponse?.DatabaseId;

                var partitionKey = resourcePermissionResponse?.ResourcePermissions?
                    .FirstOrDefault(r => r.PermissionMode == permissionMode)?.PartitionKey;

                _context = (T) Activator.CreateInstance(typeof(T), 
                    resourcePermissionResponse?.EndpointUrl, 
                    resourceToken, 
                    dbName,
                    partitionKey);

                return Task.FromResult((T) _context);

            }, permissionMode, ct);

        public override async ValueTask DisposeAsync()
        {
            _context?.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
