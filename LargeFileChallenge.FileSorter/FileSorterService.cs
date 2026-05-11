using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.UserInput;
using LargeFileChallenge.UserInput.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Drawing;

namespace LargeFileChallenge.FileSorter;

public class FileSorterService(
    IFileSplitter fileSplitter,
    IFileContentSorter fileContentSorter,
    IMultipleFileMerger multipleFileMerger,
    TextWriter textWriter,
    TextReader textReader,
    IHostApplicationLifetime lifetime,
    IConsoleFileNameProvider consoleFileNameProvider,
    IFileSizeFormatter fileSizeFormatter) : BackgroundService
{
    private readonly IFileSplitter _fileSplitter = fileSplitter;
    private readonly IFileContentSorter _fileContentSorter = fileContentSorter;
    private readonly IMultipleFileMerger _multipleFileMerger = multipleFileMerger;
    private readonly TextWriter _textWriter = textWriter;
    private readonly TextReader _textReader = textReader;
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly IConsoleFileNameProvider _consoleFileNameProvider = consoleFileNameProvider;
    private readonly IFileSizeFormatter _fileSizeFormatter = fileSizeFormatter;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay to ensure the console is ready before writing output
        await Task.Delay(1000, stoppingToken);

        var tempFolder = Path.Combine(AppContext.BaseDirectory, "tmp", Guid.NewGuid().ToString());

        var (terminate, fileName) = await _consoleFileNameProvider.GetFileName(true, stoppingToken);
        if (terminate)
        {
            _lifetime.StopApplication();
        }

        var fileInfo = new FileInfo(fileName);
        var formatted = _fileSizeFormatter.Format(fileInfo.Length);
        await _textWriter.WriteLineAsync($"\r\nInput file size: {formatted}");

        // Step 1: Split the large file into smaller chunks
        await _textWriter.WriteAsync("\r\nSplitting... ");
        var sw = Stopwatch.StartNew();
        var swTotal = Stopwatch.StartNew();
        var chunkFileNames = await _fileSplitter.SplitAsync(fileName, tempFolder, stoppingToken);
        await _textWriter.WriteLineAsync($"done in {FormatElapsed(sw.Elapsed)}");

        // Step 2: Sort each chunk in parallel
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = stoppingToken
        };
        await _textWriter.WriteAsync("\r\nSorting... ");
        sw.Restart();
        await Parallel.ForEachAsync(chunkFileNames, options, async (filename, cancellationToken) =>
        {
            await _fileContentSorter.SortAsync(filename, cancellationToken);
        });
        await _textWriter.WriteLineAsync($"done in {FormatElapsed(sw.Elapsed)}");

        // Step 3: Merge the sorted chunks into a single sorted file
        await _textWriter.WriteAsync("\r\nMerging... ");
        sw.Restart();

        var targetFileName = Path.GetFileNameWithoutExtension(fileName) + ".sorted" + Path.GetExtension(fileName);
        await _multipleFileMerger.MergeAsync(tempFolder, chunkFileNames, targetFileName, stoppingToken);
        await _textWriter.WriteLineAsync($"done in {FormatElapsed(sw.Elapsed)}");
        await _textWriter.WriteLineAsync($"----------------------------------");
        await _textWriter.WriteLineAsync($"Total: {FormatElapsed(swTotal.Elapsed)}");
        await _textWriter.WriteLineAsync($"\r\nOutput file: {targetFileName}\r\n");
        await _textReader.ReadLineAsync(stoppingToken);
        _lifetime.StopApplication();
    }

    private static string FormatElapsed(TimeSpan elapsed) =>
        elapsed.TotalSeconds < 60
            ? $"{elapsed.TotalSeconds:F2} seconds"
            : $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
}
