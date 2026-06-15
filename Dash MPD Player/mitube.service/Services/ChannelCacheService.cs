using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using mitube.service.Models;

namespace mitube.service.Services;

public class ChannelCacheService : IChannelCacheService, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly string _jsonPath;
    private readonly ILogger<ChannelCacheService> _logger;
    private FileSystemWatcher? _watcher;
    private const string CacheKey = "channel_list";

    public ChannelCacheService(IMemoryCache cache, IConfiguration config, ILogger<ChannelCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonPath = config.GetValue<string>("Channels:JsonPath") ?? "Data/canales.json";
        EnsureJsonExists();
        StartFileWatcher();
    }

    public async Task<List<ChannelGroup>> GetChannelsAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<ChannelGroup>? cached) && cached != null)
            return cached;

        var channels = await LoadFromFileAsync();

        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .RegisterPostEvictionCallback(OnCacheEvicted);

        _cache.Set(CacheKey, channels, options);
        _logger.LogInformation("Channels cache loaded from file: {Path}", _jsonPath);
        return channels;
    }

    public Task InvalidateCacheAsync()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Channels cache invalidated");
        return Task.CompletedTask;
    }

    private async Task<List<ChannelGroup>> LoadFromFileAsync()
    {
        if (!File.Exists(_jsonPath))
        {
            _logger.LogWarning("Channel JSON not found: {Path}", _jsonPath);
            return new List<ChannelGroup>();
        }

        var json = await File.ReadAllTextAsync(_jsonPath);
        var channels = JsonSerializer.Deserialize<List<ChannelGroup>>(json);

        if (channels == null)
        {
            _logger.LogError("Failed to deserialize channel JSON");
            return new List<ChannelGroup>();
        }

        _logger.LogInformation("Loaded {Count} channel groups from JSON", channels.Count);
        return channels;
    }

    private void StartFileWatcher()
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(_jsonPath));
        if (dir == null || !Directory.Exists(dir)) return;

        _watcher = new FileSystemWatcher(dir)
        {
            Filter = Path.GetFileName(_jsonPath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };

        _watcher.Changed += (_, _) =>
        {
            _logger.LogInformation("Channel JSON changed on disk, invalidating cache");
            _cache.Remove(CacheKey);
        };

        _watcher.Renamed += (_, _) =>
        {
            _logger.LogInformation("Channel JSON renamed on disk, invalidating cache");
            _cache.Remove(CacheKey);
        };
    }

    private void OnCacheEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        if (reason == EvictionReason.TokenExpired || reason == EvictionReason.Expired)
            _logger.LogDebug("Channel cache evicted: {Reason}", reason);
    }

    private void EnsureJsonExists()
    {
        var fullPath = Path.GetFullPath(_jsonPath);
        var dir = Path.GetDirectoryName(fullPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        if (!File.Exists(fullPath))
        {
            File.WriteAllText(fullPath, "[]");
            _logger.LogInformation("Created empty channel JSON: {Path}", fullPath);
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
