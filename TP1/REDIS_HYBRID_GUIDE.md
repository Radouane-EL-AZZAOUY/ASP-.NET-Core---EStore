# 🚀 Redis Hybrid Implementation Guide

## ✅ What Was Implemented

Your application now uses a **Hybrid Redis approach** that combines:
- `IDistributedCache` for basic operations (with fallback to in-memory cache)
- Direct `IConnectionMultiplexer` access for advanced Redis features

## 📦 New Features Added

### 1. Pattern-Based Cache Deletion ✨
Now fully functional! Delete multiple cache keys matching a pattern.

```csharp
// Example: Delete all product-related cache entries
await _cacheService.RemoveByPatternAsync("product:*");

// Delete all cache entries
await _cacheService.RemoveByPatternAsync("*");
```

### 2. Check Key Existence
```csharp
bool exists = await _cacheService.KeyExistsAsync("products:all");
if (exists)
{
    // Key is in cache
}
```

### 3. Count Keys by Pattern
```csharp
long count = await _cacheService.GetKeysCountAsync("product:*");
Console.WriteLine($"Found {count} product cache entries");
```

### 4. Get Time-To-Live (TTL)
```csharp
TimeSpan? ttl = await _cacheService.GetTtlAsync("products:all");
if (ttl.HasValue)
{
    Console.WriteLine($"Cache expires in {ttl.Value.TotalMinutes} minutes");
}
```

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,abortConnect=false,connectRetry=3,connectTimeout=5000"
  },
  "Redis": {
    "InstanceName": "E-Store:",
    "DefaultExpiration": "01:00:00"
  }
}
```

### Connection Settings Explained
- `abortConnect=false`: Don't throw exceptions if Redis is temporarily unavailable
- `connectRetry=3`: Retry connection 3 times
- `connectTimeout=5000`: 5 second timeout for connections

## 💻 Usage Examples

### Example 1: Enhanced Product Service
```csharp
public async Task<ProductDTO?> UpdateProductAsync(UpdateProductDTO updateDto)
{
    var product = await _unitOfWork.Products.GetByIdAsync(updateDto.Id);
    if (product == null) return null;

    _mapper.Map(updateDto, product);
    product.UpdatedAt = DateTime.Now;

    await _unitOfWork.Products.UpdateAsync(product);
    await _unitOfWork.SaveChangesAsync();

    var productDTO = _mapper.Map<ProductDTO>(product);

    // NEW: Check if cache exists before invalidating
    if (await _cacheService.KeyExistsAsync($"product:{updateDto.Id}"))
    {
        await _cacheService.RemoveAsync($"product:{updateDto.Id}");
    }
    
    // NEW: Remove all product-related cache with pattern
    await _cacheService.RemoveByPatternAsync("products:*");

    return productDTO;
}
```

### Example 2: Cache Statistics Endpoint
You could add a new endpoint to monitor cache:

```csharp
// Pages/Admin/CacheStats.cshtml.cs
public class CacheStatsModel : PageModel
{
    private readonly ICacheService _cacheService;

    public CacheStatsModel(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public long TotalKeys { get; set; }
    public long ProductKeys { get; set; }
    public TimeSpan? ProductsAllTTL { get; set; }

    public async Task OnGetAsync()
    {
        TotalKeys = await _cacheService.GetKeysCountAsync("*");
        ProductKeys = await _cacheService.GetKeysCountAsync("product*");
        ProductsAllTTL = await _cacheService.GetTtlAsync("products:all");
    }
}
```

### Example 3: Bulk Cache Invalidation
```csharp
// Clear all product caches when importing new data
public async Task ImportProductsAsync(List<Product> products)
{
    await _unitOfWork.Products.AddRangeAsync(products);
    await _unitOfWork.SaveChangesAsync();
    
    // Clear all product-related caches in one operation
    await _cacheService.RemoveByPatternAsync("product*");
    
    _logger.LogInformation("Imported {Count} products and cleared cache", products.Count);
}
```

## 🔄 Fallback Behavior

If Redis is not available:
- Basic operations (`Get`, `Set`, `Remove`) fall back to in-memory cache
- Advanced operations return safe defaults:
  - `KeyExistsAsync`: Returns false
  - `GetKeysCountAsync`: Returns 0
  - `GetTtlAsync`: Returns null
  - `RemoveByPatternAsync`: Logs warning and returns

Your application will **never crash** due to Redis being unavailable!

## 🧪 Testing the New Features

### 1. Test Pattern Deletion
```bash
# In PowerShell, check Redis keys before
docker exec $(docker ps -q -f ancestor=redis) redis-cli KEYS "E-Store:*"

# In your app, call RemoveByPatternAsync
# Then check again - keys should be gone
```

### 2. Test Key Count
```bash
# Monitor Redis in real-time
docker exec $(docker ps -q -f ancestor=redis) redis-cli MONITOR

# Use the app and watch cache operations
```

### 3. Test TTL
```bash
# Check TTL of a specific key
docker exec $(docker ps -q -f ancestor=redis) redis-cli TTL "E-Store:products:all"
```

## 📊 Architecture Benefits

### Before (IDistributedCache Only)
```
✅ Basic caching
✅ Abstraction
❌ No pattern deletion
❌ No advanced features
❌ Manual key tracking needed
```

### After (Hybrid Approach)
```
✅ Basic caching
✅ Abstraction maintained
✅ Pattern-based deletion
✅ Key existence checks
✅ TTL inspection
✅ Key counting
✅ Graceful degradation
```

## 🔥 Advanced Use Cases

### 1. Smart Cache Warming
```csharp
public async Task WarmCacheAsync()
{
    // Check if cache needs warming
    var count = await _cacheService.GetKeysCountAsync("products:*");
    if (count == 0)
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var productDTOs = _mapper.Map<List<ProductDTO>>(products);
        await _cacheService.SetAsync("products:all", productDTOs);
        _logger.LogInformation("Cache warmed with {Count} products", products.Count);
    }
}
```

### 2. Conditional Cache Refresh
```csharp
public async Task<IEnumerable<ProductDTO>> GetProductsAsync()
{
    var ttl = await _cacheService.GetTtlAsync("products:all");
    
    // Refresh cache if less than 5 minutes remaining
    if (ttl.HasValue && ttl.Value.TotalMinutes < 5)
    {
        await RefreshProductCacheAsync();
    }
    
    return await _cacheService.GetAsync<List<ProductDTO>>("products:all");
}
```

### 3. Cache Health Monitoring
```csharp
public async Task<CacheHealthStatus> GetCacheHealthAsync()
{
    var totalKeys = await _cacheService.GetKeysCountAsync("*");
    var hasProductsCache = await _cacheService.KeyExistsAsync("products:all");
    
    return new CacheHealthStatus
    {
        IsHealthy = totalKeys > 0,
        TotalKeys = totalKeys,
        CriticalCachesPresent = hasProductsCache
    };
}
```

## 🎯 Next Steps

1. **Restart your application** to load the new Redis configuration
2. **Test pattern deletion** in your product update methods
3. **Monitor cache statistics** using the new methods
4. **Consider adding** a cache admin page to monitor/clear cache
5. **Optimize TTLs** based on your data update frequency

## 📝 Summary of Changes

### Files Modified
- ✅ `Services/ICacheService.cs` - Added 3 new methods
- ✅ `Services/CacheService.cs` - Implemented hybrid Redis approach
- ✅ `Program.cs` - Registered IConnectionMultiplexer
- ✅ `appsettings.json` - Enhanced Redis configuration

### New Capabilities
- ✅ Pattern-based cache invalidation (fully functional)
- ✅ Key existence checking
- ✅ Key counting by pattern
- ✅ TTL inspection
- ✅ Graceful fallback when Redis unavailable
- ✅ Production-ready resilient connections

Your Redis implementation is now **production-ready** with enterprise-level features! 🚀

