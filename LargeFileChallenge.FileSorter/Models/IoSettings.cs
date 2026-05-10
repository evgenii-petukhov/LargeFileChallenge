namespace LargeFileChallenge.FileSorter.Models;

public class IoSettings
{
    public int ReadBufferSize { get; set; }

    public int WriteBufferSize { get; set; }

    public long ChunkSize { get; set; }

    public string? ChunkNameTemplate { get; set; }
}
