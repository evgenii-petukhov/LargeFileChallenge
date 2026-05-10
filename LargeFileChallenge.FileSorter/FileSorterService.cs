using Microsoft.Extensions.Hosting;

namespace LargeFileChallenge.FileSorter;

public class FileSorterService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
