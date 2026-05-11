namespace LargeFileChallenge.UserInput.Abstractions;

public interface IConsoleFileSizeProvider
{
    Task<(bool terminate, long size)> GetFileSize(CancellationToken cancellationToken = default);
}
