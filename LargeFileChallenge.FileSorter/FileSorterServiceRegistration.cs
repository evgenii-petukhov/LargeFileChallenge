using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using LargeFileChallenge.FileSorter.Services;
using LargeFileChallenge.UserInput;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LargeFileChallenge.FileSorter;

public static class FileSorterServiceRegistration
{
    public static IServiceCollection AddFileSorterServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IFileSplitter, FileSplitter>();
        services.AddScoped<IFileContentSorter, FileContentSorter>();
        services.AddScoped<IMultipleFileMerger, MultipleFileMerger>();
        services.Configure<IoSettings>(configuration.GetSection("IO"));
        services.AddHostedService<FileSorterService>();
        services.AddUserInputServices();

        return services;
    }
}

