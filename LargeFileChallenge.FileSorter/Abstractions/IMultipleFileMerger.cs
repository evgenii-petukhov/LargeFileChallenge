namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IMultipleFileMerger
{
    Task MergeAsync(
        string tempFolder,
        List<string> chunkFileNames,
        string outputPath,
        CancellationToken cancellationToken = default);
}
