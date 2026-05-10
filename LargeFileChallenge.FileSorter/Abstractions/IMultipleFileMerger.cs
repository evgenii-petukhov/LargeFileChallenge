namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IMultipleFileMerger
{
    Task MergeAsync(
        List<string> chunkFiles,
        string outputPath,
        CancellationToken cancellationToken = default);
}
