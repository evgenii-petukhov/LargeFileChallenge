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

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var (terminate, targetSize) = await _consoleFileSizeProvider.GetFileSize(cancellationToken);

        if (!terminate)
        {
            await _fileContentGenerator.Generate("SampleStrings.txt", "LargeFile.txt", targetSize, cancellationToken);
            await _textWriter.WriteLineAsync("File has been generated successfully");
            await _textWriter.WriteLineAsync();
        }
        _lifetime.StopApplication();
    }
}
