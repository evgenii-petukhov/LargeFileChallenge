using LargeFileChallenge.UserInput.Abstractions;
using System.Text.RegularExpressions;

namespace LargeFileChallenge.UserInput;

public partial class FileSizeParser : IFileSizeParser
{
    [GeneratedRegex(
        @"^(?<size>\d+)\s*(?<unit>B|KB|MB|GB|TB)$",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex MatchFileSizePattern();

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

            if (UserInputConstants.UnitMultipliers.TryGetValue(unit, out var multiplier))
            {
                return size * multiplier;
            }

            throw new ArgumentException($"Unsupported unit: {unit}");
        }

        throw new ArgumentException($"Invalid target size format");
    }
}
