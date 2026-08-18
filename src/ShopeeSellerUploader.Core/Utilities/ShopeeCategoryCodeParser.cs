using System.Text.RegularExpressions;

namespace ShopeeSellerUploader.Core.Utilities;

public static partial class ShopeeCategoryCodeParser
{
    public static string Normalize(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        if (trimmed.All(char.IsDigit))
        {
            return trimmed;
        }

        var matches = CategoryCodeRegex().Matches(trimmed);
        if (matches.Count == 0)
        {
            return string.Empty;
        }

        return matches
            .Select(static match => match.Value)
            .OrderByDescending(static match => match.Length)
            .First();
    }

    public static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(Normalize(value));

    [GeneratedRegex(@"\d+")]
    private static partial Regex CategoryCodeRegex();
}
