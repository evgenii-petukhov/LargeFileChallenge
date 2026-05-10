namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IFileContentSorter
{
    Task SortAsync(string filePath, CancellationToken cancellationToken = default);
}
