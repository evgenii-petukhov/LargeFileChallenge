namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IFileSplitter
{
    Task SplitAsync(
        string inputFilePath,
        string tempFolderPath,
        int chunkSize,
        CancellationToken cancellationToken = default);
}
