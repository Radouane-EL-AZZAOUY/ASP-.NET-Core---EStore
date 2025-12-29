using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TP1.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer? _redisConnection;
        private readonly ILogger<CacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _instanceName;

        public CacheService(
            IDistributedCache distributedCache, 
            ILogger<CacheService> logger,
            IConnectionMultiplexer? redisConnection = null,
            IConfiguration? configuration = null)
        {
            _distributedCache = distributedCache;
            _redisConnection = redisConnection;
            _logger = logger;
            _instanceName = configuration?["Redis:InstanceName"] ?? "E-Store:";
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedValue = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache key: {Key}", key);
                return null; // En cas d'erreur, retourner null pour éviter de bloquer l'application
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    // Durée par défaut : 1 heure
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                }

                await _distributedCache.SetStringAsync(key, serializedValue, options);
                _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expiration ?? TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
                // Ne pas lever d'exception pour éviter de bloquer l'application
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            if (_redisConnection == null)
            {
                _logger.LogWarning("Redis direct connection not available. Pattern deletion not supported: {Pattern}", pattern);
                return;
            }

            try
            {
                var db = _redisConnection.GetDatabase();
                var endpoints = _redisConnection.GetEndPoints();
                
                if (endpoints.Length == 0)
                {
                    _logger.LogWarning("No Redis endpoints available for pattern deletion");
                    return;
                }
                
                var server = _redisConnection.GetServer(endpoints.First());
                
                // Add instance name prefix to pattern
                var fullPattern = $"{_instanceName}{pattern}";
                
                var keys = server.Keys(pattern: fullPattern).ToArray();
                
                if (keys.Length > 0)
                {
                    await db.KeyDeleteAsync(keys);
                    _logger.LogInformation("Deleted {Count} keys matching pattern: {Pattern}", 
                        keys.Length, pattern);
                }
                else
                {
                    _logger.LogDebug("No keys found matching pattern: {Pattern}", pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing keys by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            if (_redisConnection == null)
            {
                // Fallback to trying to get the value
                try
                {
                    var value = await _distributedCache.GetStringAsync(key);
                    return value != null;
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                var db = _redisConnection.GetDatabase();
                var fullKey = $"{_instanceName}{key}";
                return await db.KeyExistsAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking key existence: {Key}", key);
                return false;
            }
        }

        public async Task<long> GetKeysCountAsync(string pattern)
        {
            if (_redisConnection == null)
            {
                _logger.LogWarning("Redis direct connection not available. Cannot count keys.");
                return 0;
            }

            try
            {
                var endpoints = _redisConnection.GetEndPoints();
                if (endpoints.Length == 0)
                {
                    return 0;
                }
                
                var server = _redisConnection.GetServer(endpoints.First());
                var fullPattern = $"{_instanceName}{pattern}";
                var keys = server.Keys(pattern: fullPattern);
                return keys.LongCount();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting keys with pattern: {Pattern}", pattern);
                return 0;
            }
        }

        public async Task<TimeSpan?> GetTtlAsync(string key)
        {
            if (_redisConnection == null)
            {
                _logger.LogWarning("Redis direct connection not available. Cannot get TTL.");
                return null;
            }

            try
            {
                var db = _redisConnection.GetDatabase();
                var fullKey = $"{_instanceName}{key}";
                return await db.KeyTimeToLiveAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
                return null;
            }
        }
    }
}


