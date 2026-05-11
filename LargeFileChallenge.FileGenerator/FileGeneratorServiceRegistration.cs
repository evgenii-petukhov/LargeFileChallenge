using LargeFileChallenge.FileGenerator.Abstractions;
using LargeFileChallenge.FileGenerator.Models;
using LargeFileChallenge.FileGenerator.Services;
using LargeFileChallenge.ProgressReporting;
using LargeFileChallenge.ProgressReporting.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LargeFileChallenge.FileGenerator;

public static class FileGeneratorServiceRegistration
{
    public static IServiceCollection AddFileGeneratorServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IFileContentGenerator, FileContentGenerator>();
        services.AddScoped<IFileSizeParser, FileSizeParser>();
        services.AddScoped<IConsoleFileSizeProvider, ConsoleFileSizeProvider>();
        services.AddScoped<IConsoleProgressReporter, ConsoleProgressReporter>();
        services.Configure<IoSettings>(configuration.GetSection("IO"));
        services.AddHostedService<FileGeneratorService>();

        return services;
    }
}
