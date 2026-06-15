using System.IO;
using System.Text.Json;
using DashMPDPlayer.Interfaces;
using DashMPDPlayer.Models;
using NLog;

namespace DashMPDPlayer.Services;

public class ChannelService : IChannelService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public List<ChannelGroup> LoadFromFile(string path)
    {
        _logger.Info("Cargando canales desde archivo: {Path}", path);

        if (!File.Exists(path))
        {
            _logger.Error("Archivo no encontrado: {Path}", path);
            throw new FileNotFoundException("Archivo de canales no encontrado", path);
        }

        var json = File.ReadAllText(path);
        _logger.Debug("JSON leído: {Length} chars", json.Length);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var groups = JsonSerializer.Deserialize<List<ChannelGroup>>(json, options);
        if (groups == null)
        {
            _logger.Error("La deserialización del JSON devolvió null: {Path}", path);
            throw new InvalidDataException("El archivo JSON no contiene una lista válida de canales");
        }
        var count = groups.Sum(g => g.Samples.Count);
        _logger.Info("Canales cargados: {Groups} grupos, {Channels} canales", groups.Count, count);

        return groups;
    }

    public string? FindDefaultJson(IEnumerable<string>? additionalPaths = null)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var paths = new List<string>
        {
            Path.Combine(baseDir, "canales_descifrados.json"),
            Path.Combine(baseDir, "samples", "canales_descifrados.json"),
        };
        if (additionalPaths != null)
            paths.AddRange(additionalPaths);

        foreach (var p in paths)
        {
            if (File.Exists(p))
            {
                _logger.Debug("JSON encontrado en: {Path}", p);
                return p;
            }
            _logger.Trace("JSON no encontrado en: {Path}", p);
        }

        _logger.Warn("No se encontró archivo JSON en las rutas predeterminadas");
        return null;
    }
}
