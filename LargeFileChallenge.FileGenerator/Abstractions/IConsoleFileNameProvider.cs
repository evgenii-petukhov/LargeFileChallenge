namespace LargeFileChallenge.FileGenerator.Abstractions;

public interface IConsoleFileNameProvider
{
    Task<(bool terminate, string fileName)> GetFileName(CancellationToken cancellationToken = default);
}
