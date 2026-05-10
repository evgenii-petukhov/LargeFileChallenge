using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LargeFileChallenge.FileSorter;

public static class FileSorterServiceRegistration
{
    public static IServiceCollection AddFileSorterServices(this IServiceCollection services)
    {
        services.AddHostedService<BackgroundService>();

        return services;
    }
}

