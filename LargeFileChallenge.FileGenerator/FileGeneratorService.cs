using LargeFileChallenge.FileGenerator.Abstractions;
using LargeFileChallenge.UserInput.Abstractions;
using Microsoft.Extensions.Hosting;

namespace LargeFileChallenge.FileGenerator;

public class FileGeneratorService(
    IConsoleFileSizeProvider consoleFileSizeProvider,
    IConsoleFileNameProvider consoleFileNameProvider,
    IFileContentGenerator fileContentGenerator,
    IHostApplicationLifetime lifetime,
    TextWriter textWriter,
    TextReader textReader) : BackgroundService
{
    private readonly IConsoleFileSizeProvider _consoleFileSizeProvider = consoleFileSizeProvider;
    private readonly IConsoleFileNameProvider _consoleFileNameProvider = consoleFileNameProvider;
    private readonly IFileContentGenerator _fileContentGenerator = fileContentGenerator;
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly TextWriter _textWriter = textWriter;
    private readonly TextReader _textReader = textReader;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay to ensure the console is ready before writing output
        await Task.Delay(1000, stoppingToken);
        bool terminate;
        (terminate, long targetSize) = await _consoleFileSizeProvider.GetFileSize(stoppingToken);
        if (terminate)
        {
            _lifetime.StopApplication();
        }

        (terminate, string fileName) = await _consoleFileNameProvider.GetFileName(false, stoppingToken);
        if (terminate)
        {
            _lifetime.StopApplication();
        }

        await _fileContentGenerator.GenerateAsync("SampleStrings.txt", fileName, targetSize, stoppingToken);
        await _textWriter.WriteLineAsync("\r\nFile has been generated successfully");
        await _textWriter.WriteLineAsync();

        await _textReader.ReadLineAsync(stoppingToken);
        _lifetime.StopApplication();
    }
}
