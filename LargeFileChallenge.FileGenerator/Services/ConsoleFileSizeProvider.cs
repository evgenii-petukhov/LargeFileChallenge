using LargeFileChallenge.FileGenerator.Abstractions;
using Microsoft.Extensions.Logging;

namespace LargeFileChallenge.FileGenerator.Services;

public class ConsoleFileSizeProvider(
    IFileSizeParser fileSizeParser,
    TextReader textReader,
    TextWriter textWriter,
    ILogger<ConsoleFileSizeProvider> logger) : IConsoleFileSizeProvider
{
    private readonly IFileSizeParser _fileSizeParser = fileSizeParser;
    private readonly TextReader _textReader = textReader;
    private readonly TextWriter _textWriter = textWriter;
    private readonly ILogger<ConsoleFileSizeProvider> _logger = logger;
    private readonly HashSet<string> _terminateCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXIT",
        "QUIT"
    };

    public async Task<(bool terminate, long size)> GetFileSize(CancellationToken cancellationToken = default)
    {
        var terminate = false;
        var targetSize = 0L;

        do
        {
            await _textWriter.WriteLineAsync("Enter target size (e.g., 100MB, 10GB, 1TB). Type 'exit' or 'quit' to close");
            try
            {
                var input = (await _textReader.ReadLineAsync(cancellationToken))?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    await _textWriter.WriteLineAsync("Input cannot be empty. Please enter a valid target size.");
                    continue;
                }

                if (_terminateCommands.Contains(input))
                {
                    terminate = true;
                    break;
                }

                targetSize = _fileSizeParser.Parse(input!);

                await _textWriter.WriteLineAsync(targetSize > 0
                    ? $"Target size set to {targetSize} bytes"
                    : "Please enter a positive target size");
            }
            catch (ArgumentException e)
            {
                await _textWriter.WriteLineAsync(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected exception occurred");
            }
        } while (!terminate && targetSize <= 0);

        return (terminate, targetSize);
    }
}
