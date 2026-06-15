using System.Globalization;
using System.Text;
using DashMPDPlayer.Interfaces;
using DashMPDPlayer.Models;

namespace DashMPDPlayer.Services;

public class SearchService : ISearchService
{
    public List<ChannelGroup> Filter(List<ChannelGroup> groups, string searchText)
    {
        var normalized = RemoveDiacritics(searchText.ToLowerInvariant());

        var result = new List<ChannelGroup>();
        foreach (var group in groups)
        {
            var matching = group.Samples
                .Where(c => RemoveDiacritics(c.Name?.ToLowerInvariant() ?? "").Contains(normalized))
                .ToList();

            if (matching.Count > 0)
            {
                result.Add(new ChannelGroup
                {
                    Name = group.Name,
                    Samples = matching
                });
            }
        }

        return result;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
