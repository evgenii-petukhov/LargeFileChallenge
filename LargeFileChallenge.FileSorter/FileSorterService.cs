using LargeFileChallenge.FileSorter.Abstractions;
using Microsoft.Extensions.Hosting;

namespace LargeFileChallenge.FileSorter;

public class FileSorterService(IFileSplitter fileSplitter) : BackgroundService
{
    private readonly IFileSplitter _fileSplitter = fileSplitter;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _fileSplitter.SplitAsync("LargeFile.txt", "tmp", 10 * 1024 * 1024, stoppingToken);
    }
}
