using System.Text.Json.Serialization;

namespace mitube.service.Models;

public class ChannelGroup
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("samples")]
    public List<Channel> Samples { get; set; } = new();
}
