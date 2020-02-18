using Demo1.LimitRequests;
using Demo1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo1.Code
{
    public class LimiterMiddleware
    {
        private readonly IMemoryCache _cache;
        private readonly LimiterConfig _config;
        private readonly RequestDelegate _next;
        private readonly ILogger<LimiterMiddleware> _logger;
        private readonly Limiter _limiter;

        public LimiterMiddleware(
            RequestDelegate next,
            ILogger<LimiterMiddleware> logger,
            IMemoryCache memoryCache, LimiterConfig config)
        {
            _next = next;
            _logger = logger;

            _limiter = new Limiter(memoryCache, config);
        }

        public async Task Invoke(HttpContext context, DemoContext db)
        {
            var remoteIp = context.Connection.RemoteIpAddress.ToString();
            if (!_limiter.CheckLimit(remoteIp, db))
            {
                _logger.LogInformation(
                    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = 401;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
