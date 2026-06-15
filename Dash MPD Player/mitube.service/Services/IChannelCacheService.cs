using mitube.service.Models;

namespace mitube.service.Services;

public interface IChannelCacheService
{
    Task<List<ChannelGroup>> GetChannelsAsync();
    Task InvalidateCacheAsync();
}
