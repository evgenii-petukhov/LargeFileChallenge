using LargeFileChallenge.UserInput.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LargeFileChallenge.UserInput;

public static class UserInputServiceRegistration
{
    public static IServiceCollection AddUserInputServices(this IServiceCollection services)
    {
        services.AddScoped<IFileSizeParser, FileSizeParser>();
        services.AddScoped<IFileNameValidator, FileNameValidator>();
        services.AddScoped<IConsoleFileSizeProvider, ConsoleFileSizeProvider>();
        services.AddScoped<IConsoleFileNameProvider, ConsoleFileNameProvider>();
        services.AddScoped<IFileSizeFormatter, FileSizeFormatter>();

        return services;
    }
}
