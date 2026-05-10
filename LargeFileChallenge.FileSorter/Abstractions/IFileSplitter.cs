namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IFileSplitter
{
    Task<List<string>> SplitAsync(
        string inputFilePath,
        string tempFolderPath,
        CancellationToken cancellationToken = default);
}
