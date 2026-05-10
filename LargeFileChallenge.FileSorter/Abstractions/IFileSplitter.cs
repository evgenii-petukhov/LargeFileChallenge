namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IFileSplitter
{
    Task SplitAsync(
        string inputFilePath,
        string tempFolderPath,
        long chunkSize,
        CancellationToken cancellationToken = default);
}
