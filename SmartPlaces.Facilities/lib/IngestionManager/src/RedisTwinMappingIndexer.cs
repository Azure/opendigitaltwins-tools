//-----------------------------------------------------------------------
// <copyright file="RedisTwinMappingIndexer.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IngestionManager
{
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using IngestionManager.Interfaces;
    using Polly;
    using Polly.Retry;

    public class RedisTwinMappingIndexer : ITwinMappingIndexer
    {
        private readonly AsyncRetryPolicy redisRetryPolicy;
        private readonly Random jitter;

        private readonly IDistributedCache cache;

        public RedisTwinMappingIndexer(IDistributedCache cache)
        {
            this.cache = cache;

            jitter = new Lazy<Random>().Value;

            var redisMaxRetryAttempts = 5;
            redisRetryPolicy = Policy.Handle<SocketException>()
                                     .WaitAndRetryAsync(redisMaxRetryAttempts,
                                                        (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }

        public async Task<string> GetTwinIndexAsync(string sourceId)
        {
            return await redisRetryPolicy.ExecuteAsync(async () => await cache.GetStringAsync(sourceId));
        }

        public async Task UpsertTwinIndexAsync(string sourceId, string twinId)
        {
           await redisRetryPolicy.ExecuteAsync(async () => await cache.SetStringAsync(sourceId, twinId));
        }
    }
}
