namespace LargeFileChallenge.FileGenerator.Abstractions;

public interface IConsoleProgressReporter
{
    Task UpdateProgressAsync(int currentProgress, CancellationToken cancellationToken = default);
}
