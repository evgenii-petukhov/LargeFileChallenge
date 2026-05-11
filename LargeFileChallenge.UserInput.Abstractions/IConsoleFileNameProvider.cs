namespace LargeFileChallenge.UserInput.Abstractions;

public interface IConsoleFileNameProvider
{
    Task<(bool terminate, string fileName)> GetFileName(
        bool validateFileExists,
        CancellationToken cancellationToken = default);
}
