using LargeFileChallenge.FileGenerator.Abstractions;
using LargeFileChallenge.FileGenerator.Services;
using LargeFileChallenge.ProgressReporting;
using LargeFileChallenge.ProgressReporting.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LargeFileChallenge.FileGenerator;

public static class FileGeneratorServiceRegistration
{
    public static IServiceCollection AddFileGeneratorServices(this IServiceCollection services)
    {
        services.AddScoped<IFileContentGenerator, FileContentGenerator>();
        services.AddScoped<IFileSizeParser, FileSizeParser>();
        services.AddScoped<IConsoleFileSizeProvider, ConsoleFileSizeProvider>();
        services.AddScoped<IConsoleProgressReporter, ConsoleProgressReporter>();
        services.AddHostedService<FileGeneratorService>();

        return services;
    }
}
