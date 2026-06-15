using DashMPDPlayer.Models;

namespace DashMPDPlayer.Interfaces;

public interface ISearchService
{
    List<ChannelGroup> Filter(List<ChannelGroup> groups, string searchText);
}
