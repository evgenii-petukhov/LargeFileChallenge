namespace LargeFileChallenge.FileGenerator.Abstractions;

public interface IFileContentGenerator
{
    Task Generate(
        string sampleFilePath,
        string targetFilePath,
        long targetSize,
        CancellationToken cancellationToken = default);
}
