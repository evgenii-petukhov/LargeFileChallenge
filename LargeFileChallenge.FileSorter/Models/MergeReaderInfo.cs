namespace LargeFileChallenge.FileSorter.Models;

public class MergeReaderInfo(BinaryReader reader, int index)
{
    public BinaryReader? Reader { get; set; } = reader;

    public int Index { get; set; } = index;
}
