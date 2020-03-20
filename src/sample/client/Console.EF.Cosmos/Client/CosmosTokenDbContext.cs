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
    public class CosmosTokenDbContext<T> : BrokerClientHandler where T : IAsyncDisposable
    {
        private T _context;
        
        public CosmosTokenDbContext(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey resourceTokenCache = null) : base(authService, resourceTokenBrokerUrl, resourceTokenCache)
        {

        }

        public async Task<T> GetDbContextAsync(PermissionModeKind permissionMode, CancellationToken ct = default) =>
            await ExecuteCommandWrapper(resourcePermissionResponse =>
            {
                var permission = resourcePermissionResponse?.ResourcePermissions?
                    .FirstOrDefault(r => r.PermissionMode == permissionMode);
                                               
                _context = (T) Activator.CreateInstance(typeof(T), 
                    resourcePermissionResponse?.EndpointUrl,
                    permission?.ResourceToken,
                    resourcePermissionResponse?.DatabaseId,
                    permission?.PartitionKey);

                return Task.FromResult(_context);

            }, permissionMode, ct);

        public override async ValueTask DisposeAsync()
        {
            _context?.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
