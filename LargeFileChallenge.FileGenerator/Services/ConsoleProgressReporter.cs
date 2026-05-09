using LargeFileChallenge.FileGenerator.Abstractions;

namespace LargeFileChallenge.FileGenerator.Services;

public class ConsoleProgressReporter(TextWriter textWriter) : IConsoleProgressReporter
{
    private readonly TextWriter _textWriter = textWriter;
    private int _progress = 0;
    private int _lastMessageLength = 0;

    public async Task UpdateProgressAsync(int currentProgress, CancellationToken cancellationToken = default)
    {
        if (currentProgress < _progress + 10) return;

        _progress = currentProgress;

        var message = $"Progress: {_progress}%";
        var padding = _lastMessageLength > message.Length ? new string(' ', _lastMessageLength - message.Length) : string.Empty;
        _lastMessageLength = message.Length;
        await _textWriter.WriteAsync("\r" + message + padding);
        await _textWriter.FlushAsync(cancellationToken);

        if (_progress >= 100)
        {
            await _textWriter.WriteLineAsync();
        }
    }
}
