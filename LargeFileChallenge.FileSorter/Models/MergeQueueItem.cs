namespace LargeFileChallenge.FileSorter.Models;

public readonly struct MergeQueueItem(int readerIndex, int number, string text)
{
    public int ReaderIndex { get; } = readerIndex;

    public int Number { get; } = number;

    public string Text { get; } = text;
}