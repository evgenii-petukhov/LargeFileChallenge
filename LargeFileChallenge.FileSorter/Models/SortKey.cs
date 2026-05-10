namespace LargeFileChallenge.FileSorter.Models;

public sealed record SortKey(string Text, int Number) : IComparable<SortKey>
{
    public int CompareTo(SortKey? other)
    {
        if (other is null)
        {
            return 1;
        }

        var compareResult = string.CompareOrdinal(Text, other.Text);

        return compareResult == 0
            ? Number.CompareTo(other.Number)
            : compareResult;
    }
}