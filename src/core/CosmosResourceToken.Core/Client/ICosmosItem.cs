﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosResourceToken.Core.Client
{
    public interface ICosmosItem<T> 
    {
        T Document { get; set; }

        string Id { get; set; }

        Task<Stream> ToStream(string partitionKeyHeader, string partitionKey, CancellationToken ct = default);

        Task<ICosmosItem<T>> GetObjectFromStream(Stream stream, CancellationToken ct = default);

        Task<IEnumerable<string>> GetJsonStringsFromStream(Stream stream, CancellationToken ct = default);
    }
}
