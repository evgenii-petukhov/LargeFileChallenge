using LargeFileChallenge.UserInput.Abstractions;
using Microsoft.Extensions.Logging;

namespace LargeFileChallenge.UserInput;

public class ConsoleFileNameProvider(
    IFileNameValidator fileNameValidator,
    TextReader textReader,
    TextWriter textWriter,
    ILogger<ConsoleFileSizeProvider> logger) : IConsoleFileNameProvider
{
    private readonly IFileNameValidator _fileNameValidator = fileNameValidator;
    private readonly TextReader _textReader = textReader;
    private readonly TextWriter _textWriter = textWriter;
    private readonly ILogger<ConsoleFileSizeProvider> _logger = logger;

    public async Task<(bool terminate, string fileName)> GetFileName(
        bool validateFileExists,
        CancellationToken cancellationToken = default)
    {
        var terminate = false;
        var isValid = false;
        string input = null!;

        do
        {
            await _textWriter.WriteLineAsync("\r\nEnter file name or full path");
            try
            {
                input = (await _textReader.ReadLineAsync(cancellationToken))?.Trim()!;

                if (string.IsNullOrWhiteSpace(input))
                {
                    await _textWriter.WriteLineAsync("\r\nFile name cannot be empty. Please try again.");
                    continue;
                }

                if (!_fileNameValidator.IsValid(input))
                {
                    await _textWriter.WriteLineAsync("\r\nInvalid file name. Please enter a valid file name or full path.");
                    continue;
                }

                if (validateFileExists && !File.Exists(input))
                {
                    await _textWriter.WriteLineAsync("\r\nThe specified file doesn't exist.");
                    continue;
                }

                if (UserInputConstants.TerminateCommands.Contains(input))
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

        return (terminate, input!);
    }
}
