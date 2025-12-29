namespace TP1.Services
{
    public interface ICacheService
    {
        // Basic cache operations
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        
        // Redis-specific advanced operations
        Task<bool> KeyExistsAsync(string key);
        Task<long> GetKeysCountAsync(string pattern);
        Task<TimeSpan?> GetTtlAsync(string key);
    }
}


