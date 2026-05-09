namespace LargeFileChallenge.FileGenerator.Abstractions;

public interface IFileContentGenerator
{
    Task GenerateAsync(
        string sampleFilePath,
        string targetFilePath,
        long targetSize,
        CancellationToken cancellationToken = default);
}
