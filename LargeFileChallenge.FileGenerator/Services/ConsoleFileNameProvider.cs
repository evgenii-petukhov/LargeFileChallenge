
using LargeFileChallenge.FileGenerator.Abstractions;
using Microsoft.Extensions.Logging;

namespace LargeFileChallenge.FileGenerator.Services;

public class ConsoleFileNameProvider(
    IFileNameValidator fileNameValidator,
    TextReader textReader,
    TextWriter textWriter,
    ILogger<ConsoleFileSizeProvider> logger) : IConsoleFileNameProvider
{
    private static readonly HashSet<string> _terminateCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXIT",
        "QUIT"
    };

    private readonly IFileNameValidator _fileNameValidator = fileNameValidator;
    private readonly TextReader _textReader = textReader;
    private readonly TextWriter _textWriter = textWriter;
    private readonly ILogger<ConsoleFileSizeProvider> _logger = logger;

    public async Task<(bool terminate, string fileName)> GetFileName(CancellationToken cancellationToken = default)
    {
        var terminate = false;
        var isValid = false;
        string fileName = null!;

        do
        {
            await _textWriter.WriteLineAsync("\r\nEnter the output file name or full path:");
            await _textWriter.WriteLineAsync("Type 'exit' or 'quit' to cancel.");
            try
            {
                fileName = (await _textReader.ReadLineAsync(cancellationToken))?.Trim()!;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    await _textWriter.WriteLineAsync("\r\nFile name cannot be empty. Please try again.");
                    continue;
                }

                if (!_fileNameValidator.IsValid(fileName))
                {
                    await _textWriter.WriteLineAsync("\r\nInvalid file name. Please enter a valid file name or full path.");
                    continue;
                }

                if (_terminateCommands.Contains(fileName))
                {
                    terminate = true;
                    break;
                }

                isValid = true;
            }
            catch (ArgumentException e)
            {
                await _textWriter.WriteLineAsync("\r\n" + e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "\r\nAn unexpected exception occurred");
            }
        } while (!terminate && !isValid);

        return (terminate, fileName!);
    }
}
