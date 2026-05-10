namespace LargeFileChallenge.FileSorter.Models;

public readonly struct MergeQueueItem(int readerIndex, string line)
{
    public int ReaderIndex { get; } = readerIndex;

    public string Line { get; } = line;
}