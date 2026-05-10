using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LargeFileChallenge.FileSorter;

public static class FileSorterServiceRegistration
{
    public static IServiceCollection AddFileSorterServices(this IServiceCollection services)
    {
        services.AddScoped<IFileSplitter, FileSplitter>();
        services.AddHostedService<FileSorterService>();

        return services;
    }
}

