namespace DashMPDPlayer.Models;

public record DrmInfo(string SystemId, string LicenseUrl)
{
    public bool IsWidevine => SystemId.Equals("edef8ba9-79d6-4ace-a3c8-27dcd51d21ed", StringComparison.OrdinalIgnoreCase);
    public bool IsPlayReady => SystemId.Equals("9a04f079-9840-4286-ab92-e65be0885f95", StringComparison.OrdinalIgnoreCase);
    public bool IsClearKey => SystemId.Equals("e2719d58-a985-b3c9-781a-b030af78d30e", StringComparison.OrdinalIgnoreCase);
}
