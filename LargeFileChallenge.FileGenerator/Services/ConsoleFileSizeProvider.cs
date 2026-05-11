using LargeFileChallenge.FileGenerator.Abstractions;
using Microsoft.Extensions.Logging;

namespace LargeFileChallenge.FileGenerator.Services;

public class ConsoleFileSizeProvider(
    IFileSizeParser fileSizeParser,
    TextReader textReader,
    TextWriter textWriter,
    ILogger<ConsoleFileSizeProvider> logger) : IConsoleFileSizeProvider
{
    private static readonly HashSet<string> _terminateCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXIT",
        "QUIT"
    };

    private readonly IFileSizeParser _fileSizeParser = fileSizeParser;
    private readonly TextReader _textReader = textReader;
    private readonly TextWriter _textWriter = textWriter;
    private readonly ILogger<ConsoleFileSizeProvider> _logger = logger;

    public async Task<(bool terminate, long size)> GetFileSize(CancellationToken cancellationToken = default)
    {
        var terminate = false;
        var targetSize = 0L;

        do
        {
            await _textWriter.WriteLineAsync("\r\nEnter the target file size (e.g. 100MB, 10GB, 1TB):");
            await _textWriter.WriteLineAsync("Type 'exit' or 'quit' to cancel.");
            try
            {
                var input = (await _textReader.ReadLineAsync(cancellationToken))?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    await _textWriter.WriteLineAsync("\r\nSize cannot be empty. Please try again.");
                    continue;
                }

                if (_terminateCommands.Contains(input))
                {
                    terminate = true;
                    break;
                }

                targetSize = _fileSizeParser.Parse(input!);

                if (targetSize <= 0)
                {
                    await _textWriter.WriteLineAsync("\r\nSize must be greater than zero. Please try again.");
                }
            }
            catch (ArgumentException e)
            {
                await _textWriter.WriteLineAsync("\r\n" + e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "\r\nAn unexpected exception occurred");
            }
        } while (!terminate && targetSize <= 0);

        return (terminate, targetSize);
    }
}
