using LargeFileChallenge.FileGenerator.Abstractions;
using LargeFileChallenge.FileGenerator.Models;
using LargeFileChallenge.ProgressReporting.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;

namespace LargeFileChallenge.FileGenerator.Services;

public class FileContentGenerator(
    IConsoleProgressReporter consoleProgressReporter,
    IOptions<IoSettings> options) : IFileContentGenerator
{
    private readonly Random _random = new((int)DateTime.Now.Ticks & 0x0000FFFF);
    private readonly IConsoleProgressReporter _consoleProgressReporter = consoleProgressReporter;
    private readonly IoSettings _ioSettings = options.Value;

    public async Task GenerateAsync(
        string sampleFilePath,
        string targetFilePath,
        long targetSize,
        CancellationToken cancellationToken = default)
    {
        if (targetSize <= 0)
        {
            throw new ArgumentException("The target size must be greater than zero.", nameof(targetSize));
        }

        var sampleStrings = await File.ReadAllLinesAsync(sampleFilePath, cancellationToken);

        var currentSize = 0L;

        await using var stream = new FileStream(
            targetFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: _ioSettings.WriteBufferSize,
            options: FileOptions.Asynchronous);

        while (currentSize < targetSize)
        {
            var shuffledStringsEnumerable = sampleStrings
                .Select(s => $"{_random.Next()}. {s}")
                .OrderBy(_ => _random.Next());

            var textChunk = string.Join(Environment.NewLine, shuffledStringsEnumerable) + Environment.NewLine;

            var bytes = Encoding.UTF8.GetBytes(textChunk);

            await stream.WriteAsync(bytes, cancellationToken);

            currentSize += bytes.Length;

            await _consoleProgressReporter.UpdateProgressAsync((int)(currentSize * 100m / targetSize), cancellationToken);
        }

        if (currentSize >= targetSize)
        {
            await _consoleProgressReporter.UpdateProgressAsync(100, cancellationToken);
        }
    }
}
