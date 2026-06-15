using DashMPDPlayer.Models;
using System.Threading.Tasks;

namespace DashMPDPlayer.Interfaces;

public interface IProxyService : IDisposable
{
    int Port { get; }
    string ProxyBaseUrl { get; }
    event Action<string>? OnLog;
    void SetPlayerHtml(string html);
    void SetChannelConfig(string mpdUrl, Dictionary<string, string>? headers);
    Task<DrmInfo?> DetectDrmAsync(string mpdUrl, Dictionary<string, string>? headers);
    void Start();
    void Stop();
}
