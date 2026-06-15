using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mitube.service.Models;
using mitube.service.Services;

namespace mitube.service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChannelsController : ControllerBase
{
    private readonly IChannelCacheService _channelCache;
    private readonly IConfiguration _config;
    private readonly ILogger<ChannelsController> _logger;

    public ChannelsController(IChannelCacheService channelCache, IConfiguration config, ILogger<ChannelsController> logger)
    {
        _channelCache = channelCache;
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await _channelCache.GetChannelsAsync();
        return Ok(channels);
    }

    [HttpPut("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadChannels([FromBody] List<ChannelGroup> channelGroups)
    {
        if (channelGroups == null || channelGroups.Count == 0)
            return BadRequest(new { error = "Invalid channel data" });

        var jsonPath = _config.GetValue<string>("Channels:JsonPath") ?? "Data/canales.json";
        var json = JsonSerializer.Serialize(channelGroups, new JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(jsonPath, json);

        await _channelCache.InvalidateCacheAsync();

        _logger.LogInformation("Channels updated via upload: {Count} groups", channelGroups.Count);
        return Ok(new { message = "Channels updated", count = channelGroups.Count });
    }

    [HttpPost("reload")]
    public async Task<IActionResult> ReloadFromDisk()
    {
        await _channelCache.InvalidateCacheAsync();
        var channels = await _channelCache.GetChannelsAsync();
        _logger.LogInformation("Channels reloaded from disk");
        return Ok(new { message = "Cache invalidated and reloaded", count = channels.Count });
    }
}
