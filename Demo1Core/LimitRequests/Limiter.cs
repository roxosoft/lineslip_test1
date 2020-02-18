using Demo1.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo1.LimitRequests
{
    public struct LimiterConfig
    {
        public int RequestsLimit { get; set; }
        public TimeSpan RequestsInterval { get; set; }
        public TimeSpan DbCacheInterval { get; set; }
    }

    public class Limiter
    {
        private readonly IMemoryCache _cache;
        private readonly LimiterConfig _config;

        public Limiter(IMemoryCache memoryCache, LimiterConfig config)
        {
            _cache = memoryCache;
            _config = config;
        }

        string GetBanKey(string ip) => $"ban_{ip}";
        string GetThrottlerKey(string ip) => $"throttle_{ip}";

        public bool CheckLimit(string ip, DemoContext db)
        {
            //look at the IPs blocked / allowed cache record
            if (!_cache.TryGetValue<bool>(GetBanKey(ip), out var banned))
            {
                banned = db.BlockedIps.Any(p => p.Ip == ip);

                SetBanInCache(ip, banned);
            }

            if (!banned)
            {
                // Look for cache key.
                if (!_cache.TryGetValue<Throttler>(GetThrottlerKey(ip), out var cacheEntry))
                {
                    // Key not in cache, so get data.
                    cacheEntry = new Throttler(_config.RequestsLimit, _config.RequestsInterval, _config.RequestsLimit);

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(_config.RequestsInterval);

                    // Save data in cache.
                    _cache.Set(GetThrottlerKey(ip), cacheEntry, cacheEntryOptions);
                }

                var result = cacheEntry.TryThrottledWait(1);
                if (!result)
                {
                    SetBanInCache(ip, true);
                    Ban(ip, db);
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        void SetBanInCache(string ip, bool banned)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_config.DbCacheInterval);

            _cache.Set(GetBanKey(ip), banned, cacheEntryOptions);
        }

        void Ban(string ip, DemoContext db)
        {
            if (!db.BlockedIps.Any(p => p.Ip == ip))
            {
                db.BlockedIps.Add(new BlockedIps() { Ip = ip, Date = DateTime.UtcNow });
                db.SaveChanges();
            }
        }
    }
}
