using LargeFileChallenge.FileGenerator.Abstractions;
using System.Text.RegularExpressions;

namespace LargeFileChallenge.FileGenerator.Services;

public partial class FileSizeParser : IFileSizeParser
{
    [GeneratedRegex(
        @"^(?<size>\d+)\s*(?<unit>B|KB|MB|GB|TB)$",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex MatchFileSizePattern();

    private static readonly Dictionary<string, long> _unitMultipliers = new(StringComparer.Ordinal)
    {
        { "B", 1L },
        { "KB", 1024L },
        { "MB", 1024L * 1024L },
        { "GB", 1024L * 1024L * 1024L },
        { "TB", 1024L * 1024L * 1024L * 1024L }
    };

    public long Parse(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var match = MatchFileSizePattern().Match(input.Trim().ToUpper());

        if (match.Success)
        {
            var sizeString = match.Groups["size"].Value;
            if (!int.TryParse(sizeString, out var size))
            {
                throw new ArgumentException($"Invalid size value: {sizeString}");
            }
            var unit = match.Groups["unit"].Value;

            if (_unitMultipliers.TryGetValue(unit, out var multiplier))
            {
                return size * multiplier;
            }

            throw new ArgumentException($"Unsupported unit: {unit}");
        }

        throw new ArgumentException($"Invalid target size format");
    }
}
