using System.Text;
using FilarScraper.Models;
using Microsoft.Extensions.Logging;

namespace FilarScraper.Services;

public class CsvExporterService : ICsvExporterService
{
    private readonly ILogger<CsvExporterService> _logger;

    public CsvExporterService(ILogger<CsvExporterService> logger)
    {
        _logger = logger;
    }

    public async Task ExportAsync(
        ScraperResult result,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting CSV to: {Path}", filePath);

        var sb = new StringBuilder();

        // Header
        sb.AppendLine("ProductName,ColorName,HexColor,AttributeValueId,ImageFileName,ImageUrl,LocalPath,Downloaded");

        foreach (var variant in result.Variants)
        {
            if (variant.Images.Count == 0)
            {
                sb.AppendLine(BuildRow(
                    result.ProductName,
                    variant.ColorName,
                    variant.HexColor,
                    variant.AttributeValueId,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    false));
                continue;
            }

            foreach (var image in variant.Images)
            {
                sb.AppendLine(BuildRow(
                    result.ProductName,
                    variant.ColorName,
                    variant.HexColor,
                    variant.AttributeValueId,
                    image.FileName,
                    image.Url,
                    image.LocalPath,
                    image.Downloaded));
            }
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, cancellationToken);
        _logger.LogInformation("CSV exported successfully: {Path}", filePath);
    }

    private static string BuildRow(
        string productName,
        string colorName,
        string hexColor,
        int attributeValueId,
        string fileName,
        string imageUrl,
        string localPath,
        bool downloaded)
    {
        return string.Join(",",
            Escape(productName),
            Escape(colorName),
            Escape(hexColor),
            attributeValueId,
            Escape(fileName),
            Escape(imageUrl),
            Escape(localPath),
            downloaded.ToString().ToLowerInvariant());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
