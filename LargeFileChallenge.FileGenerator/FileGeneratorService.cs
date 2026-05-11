using LargeFileChallenge.FileGenerator.Abstractions;
using Microsoft.Extensions.Hosting;

namespace LargeFileChallenge.FileGenerator;

public class FileGeneratorService(
    IConsoleFileSizeProvider consoleFileSizeProvider,
    IFileContentGenerator fileContentGenerator,
    IHostApplicationLifetime lifetime,
    TextWriter textWriter) : BackgroundService
{
    private readonly IConsoleFileSizeProvider _consoleFileSizeProvider = consoleFileSizeProvider;
    private readonly IFileContentGenerator _fileContentGenerator = fileContentGenerator;
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly TextWriter _textWriter = textWriter;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay to ensure the console is ready before writing output
        await Task.Delay(1000, stoppingToken);
        var (terminate, targetSize) = await _consoleFileSizeProvider.GetFileSize(stoppingToken);

        if (!terminate)
        {
            await _fileContentGenerator.GenerateAsync("SampleStrings.txt", "LargeFile.txt", targetSize, stoppingToken);
            await _textWriter.WriteLineAsync("File has been generated successfully");
            await _textWriter.WriteLineAsync();
        }
        _lifetime.StopApplication();
    }
}
