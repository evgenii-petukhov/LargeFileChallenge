using LargeFileChallenge.FileSorter.Abstractions;
using Microsoft.Extensions.Hosting;

namespace LargeFileChallenge.FileSorter;

public class FileSorterService(
    IFileSplitter fileSplitter,
    IFileContentSorter fileContentSorter,
    TextWriter textWriter) : BackgroundService
{
    private readonly IFileSplitter _fileSplitter = fileSplitter;
    private readonly IFileContentSorter _fileContentSorter = fileContentSorter;
    private readonly TextWriter _textWriter = textWriter;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _textWriter.WriteLineAsync("Splitting...");
        var chunkFileNames = await _fileSplitter.SplitAsync("LargeFile.txt", "tmp", stoppingToken);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = stoppingToken
        };

        await _textWriter.WriteLineAsync("Sorting...");
        await Parallel.ForEachAsync(chunkFileNames, options, async (filename, cancellationToken) =>
        {
            await _fileContentSorter.SortAsync(filename, cancellationToken);
        });

        await _textWriter.WriteLineAsync("Done");
    }
}
