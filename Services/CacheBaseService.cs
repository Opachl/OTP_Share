using Microsoft.Extensions.Caching.Memory;

namespace OTP_Share.Services
{
  public abstract class CacheBaseService
  {
    private readonly ILogger _logger;
    private readonly MemoryCache _cache;

    public CacheBaseService(ILogger logger)
      : this(logger, TimeSpan.FromMinutes(10))
    { }

    public CacheBaseService(ILogger logger, TimeSpan cacheDuration)
    {
      _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public T Get<T>(string key)
    {
      T result = default(T);

      if(_cache.TryGetValue<T>(key, out T cResult))
      {
        _logger.LogInformation($"Cache hit for key: {key}");
        result = cResult;
      }

      return result;
    }

    public T Set<T>(string key, T value)
    {
      _logger.LogInformation($"Cache set for key: {key}");
      return _cache.Set(key, value, TimeSpan.FromMinutes(5));
    }
  }
}