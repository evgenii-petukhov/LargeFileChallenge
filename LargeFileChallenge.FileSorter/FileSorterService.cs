using LargeFileChallenge.FileSorter.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace LargeFileChallenge.FileSorter;

public class FileSorterService(
    IFileSplitter fileSplitter,
    IFileContentSorter fileContentSorter,
    IMultipleFileMerger multipleFileMerger,
    TextWriter textWriter,
    TextReader textReader,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    private readonly IFileSplitter _fileSplitter = fileSplitter;
    private readonly IFileContentSorter _fileContentSorter = fileContentSorter;
    private readonly IMultipleFileMerger _multipleFileMerger = multipleFileMerger;
    private readonly TextWriter _textWriter = textWriter;
    private readonly TextReader _textReader = textReader;
    private readonly IHostApplicationLifetime _lifetime = lifetime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay to ensure the console is ready before writing output
        await Task.Delay(1000, stoppingToken);

        const string inputFilePath = "LargeFile.txt";
        var fileInfo = new FileInfo(inputFilePath);
        await _textWriter.WriteLineAsync($"\r\nInput file size: {fileInfo.Length}");

        // Step 1: Split the large file into smaller chunks
        await _textWriter.WriteAsync("\r\nSplitting... ");
        var sw = Stopwatch.StartNew();
        var swTotal = Stopwatch.StartNew();
        var chunkFileNames = await _fileSplitter.SplitAsync(inputFilePath, "tmp", stoppingToken);
        var elapsed = sw.Elapsed.TotalSeconds;
        await _textWriter.WriteLineAsync($"done in {elapsed:F2} seconds");

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
        elapsed = sw.Elapsed.TotalSeconds;
        await _textWriter.WriteLineAsync($"done in {elapsed:F2} seconds");

        // Step 3: Merge the sorted chunks into a single sorted file
        await _textWriter.WriteAsync("\r\nMerging... ");
        sw.Restart();
        await _multipleFileMerger.MergeAsync(chunkFileNames, "LargeFile.sorted.txt", stoppingToken);
        elapsed = sw.Elapsed.TotalSeconds;
        await _textWriter.WriteLineAsync($"done in {elapsed:F2} seconds");

        var elapsedTotal = swTotal.Elapsed.TotalSeconds;
        await _textWriter.WriteLineAsync($"\r\nTotal: {elapsedTotal:F2} seconds\r\n");
        await _textReader.ReadLineAsync(stoppingToken);
        _lifetime.StopApplication();
    }
}
