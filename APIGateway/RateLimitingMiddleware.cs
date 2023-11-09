using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;//TODO: Replace it with Redis cache
using System;
using System.Threading.Tasks;

namespace APIGateway
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache; //TODO: Repalce it with Redis cache
        private readonly int _requestRateLimit = 100;

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress.ToString();

            var cacheKey = $"RateLimit_{ipAddress}";

            if (!_cache.TryGetValue(cacheKey, out int requestCount))
            {
                requestCount = 0;
            }

            if (requestCount >= _requestRateLimit) // Allowing requests defined in _rateLimit per minute from the single IP
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            requestCount++;
            _cache.Set(cacheKey, requestCount, TimeSpan.FromSeconds(60)); // Last request to next request time

            await _next(context);
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}


