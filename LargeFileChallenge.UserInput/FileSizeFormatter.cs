using LargeFileChallenge.UserInput.Abstractions;

namespace LargeFileChallenge.UserInput;

public class FileSizeFormatter : IFileSizeFormatter
{
    public string Format(long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentException("Bytes cannot be negative.", nameof(bytes));
        }

        foreach (var (unit, multiplier) in UserInputConstants.UnitMultipliers.Reverse())
        {
            if (bytes >= multiplier)
            {
                var value = bytes / multiplier;
                return bytes % multiplier == 0
                    ? $"{value}{unit}"
                    : $"{(double)bytes / multiplier:0.##}{unit}";
            }
        }

        return "0B";
    }
}
