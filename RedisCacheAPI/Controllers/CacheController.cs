using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisCacheAPI.Services;

namespace RedisCacheAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        // GET: api/cache/{key}
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            var value = await _cacheService.GetAsync<object>(key);

            if (value == null)
                return NotFound(new { message = $"Key '{key}' not found in cache" });

            return Ok(new { key, value });
        }

        // POST: api/cache
        [HttpPost]
        public async Task<IActionResult> Set([FromBody] CacheRequest request)
        {
            if (string.IsNullOrEmpty(request.Key))
                return BadRequest(new { message = "Key is required" });

            TimeSpan? expiration = request.ExpirationMinutes.HasValue
                ? TimeSpan.FromMinutes(request.ExpirationMinutes.Value)
                : null;

            await _cacheService.SetAsync(request.Key, request.Value, expiration);

            return Ok(new { message = "Value cached successfully", key = request.Key });
        }

        // DELETE: api/cache/{key}
        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            await _cacheService.RemoveAsync(key);
            return Ok(new { message = $"Key '{key}' removed from cache" });
        }

        // GET: api/cache/exists/{key}
        [HttpGet("exists/{key}")]
        public async Task<IActionResult> Exists(string key)
        {
            var exists = await _cacheService.ExistsAsync(key);
            return Ok(new { key, exists });
        }

        // GET: api/cache/health
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Redis cache is configured", timestamp = DateTime.UtcNow });
        }
    }

    public class CacheRequest
    {
        public string Key { get; set; } = string.Empty;
        public object Value { get; set; } = new();
        public int? ExpirationMinutes { get; set; }
    }
}