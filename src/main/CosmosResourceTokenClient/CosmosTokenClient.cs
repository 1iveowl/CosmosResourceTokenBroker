using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using B2CAuthClient.Abstract;
using CosmosResourceToken.Core.Client;
using CosmosResourceToken.Core.Model;

namespace CosmosResourceTokenClient
{
    /// <summary>
    ///     <para>
    ///         Cosmos token client.
    ///     </para>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class CosmosTokenClient : BrokerClientExecutionHandler, ICosmosTokenClient, IAsyncDisposable
    {
        private readonly BrokerClientExecutionHandler _cosmosClientHandler;

        /// <summary>
        ///     <para>
        ///         Instance of Cosmos token client.
        ///     </para>
        /// </summary>
        /// <param name="authService">Azure AD B2C authentication service.</param>
        /// <param name="resourceTokenBrokerUrl">Url for the resource token broker</param>
        /// <param name="resourceTokenCache">Caching of token</param>
        public CosmosTokenClient(
            IB2CAuthService authService, 
            string resourceTokenBrokerUrl,
            ICacheSingleObjectByKey resourceTokenCache = null) : base(authService, resourceTokenBrokerUrl, resourceTokenCache)
        {

        }

        #region Stream API

        /// <summary>
        ///     <para>
        ///         Create Cosmos document.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document to create.</typeparam>
        /// <param name="id">Unique id of document.</param>
        /// <param name="item">The instance of document.</param>
        /// <param name="defaultPartition">Partition used (user or shared)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task Create<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Shared)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientStreamWrapper(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                await cosmosClientEx.Create(id, item, cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        /// <summary>
        ///     <para>
        ///         Replace/Upsert document on Cosmos DB.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document to create.</typeparam>
        /// <param name="id">Unique id of document.</param>
        /// <param name="item">The instance of document.</param>
        /// <param name="defaultPartition">Partition used (user or shared)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task Replace<T>(string id, T item, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Shared)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientStreamWrapper(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                await cosmosClientEx.Replace(id, item, cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        /// <summary>
        ///     <para>
        ///         Read document from Cosmos DB.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document to create.</typeparam>
        /// <param name="id">Unique id of document.</param>
        /// <param name="defaultPartition">Partition used (user or shared)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Document of type T</returns>
        public async Task<T> Read<T>(
            string id, 
            DefaultPartitionKind defaultPartition, 
            CancellationToken cancellationToken = default) =>
                await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
                {
                    var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                        ? PermissionModeKind.UserRead
                        : PermissionModeKind.SharedRead;

                    await using var cosmosClientEx = new CosmosClientStreamWrapper(resourcePermissionResponse, permissionMode);

                    return await cosmosClientEx.Read<T>(id, cancellationToken);

                }, PermissionModeKind.UserReadWrite, cancellationToken);

        /// <summary>
        ///     <para>
        ///         Delete document from Cosmos DB.
        ///     </para>
        /// </summary>
        /// <param name="id">Unique id of document.</param>
        /// <param name="defaultPartition">Partition used (user or shared)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task Delete(string id, DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                if (defaultPartition == DefaultPartitionKind.Shared)
                {
                    throw new CosmosClientException("Users has read only access to the Global partition.");
                }

                await using var cosmosClientEx = new CosmosClientStreamWrapper(resourcePermissionResponse, PermissionModeKind.UserReadWrite);

                await cosmosClientEx.Delete(id, cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        /// <summary>
        ///     <para>
        ///         Get list of all document of type T in users partition from Cosmos DB.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of document to create.</typeparam>
        /// <param name="defaultPartition">Partition used (user or shared)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable list of type T.</returns>
        public async Task<IEnumerable<T>> List<T>(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                    ? PermissionModeKind.UserRead
                    : PermissionModeKind.SharedRead;

                await using var cosmosClientEx = new CosmosClientStreamWrapper(resourcePermissionResponse, permissionMode);

                return await cosmosClientEx.GetPartitionObjects<T>(cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);

        /// <summary>
        ///     <para>
        ///         Get list of all document in users partition from Cosmos DB as json string.
        ///     </para>
        /// </summary>
        /// <param name="defaultPartition">Partition used (user or shared)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable list of json strings.</returns>
        public async Task<IEnumerable<string>> GetPartitionDocuments(DefaultPartitionKind defaultPartition, CancellationToken cancellationToken = default) =>
            await _cosmosClientHandler.Execute(async resourcePermissionResponse =>
            {
                var permissionMode = defaultPartition == DefaultPartitionKind.UserDocument
                    ? PermissionModeKind.UserRead
                    : PermissionModeKind.SharedRead;

                await using var cosmosClientEx = new CosmosClientStreamWrapper(resourcePermissionResponse, permissionMode);

                return await cosmosClientEx.GetPartitionDocuments(cancellationToken);

            }, PermissionModeKind.UserReadWrite, cancellationToken);


        public override async ValueTask DisposeAsync()
        {
            await _cosmosClientHandler.DisposeAsync();
            await base.DisposeAsync();
        }

        #endregion
    }
}
