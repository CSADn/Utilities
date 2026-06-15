using DashMPDPlayer.Models;
using System.Collections.Generic;

namespace DashMPDPlayer.Interfaces;

public interface IChannelService
{
    List<ChannelGroup> LoadFromFile(string path);
    string? FindDefaultJson(IEnumerable<string>? additionalPaths = null);
}
