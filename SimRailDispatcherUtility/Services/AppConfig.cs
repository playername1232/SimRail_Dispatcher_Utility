namespace SimRailDispatcherUtility.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

public static class AppConfig
{
    public static IConfiguration Config { get; private set; } = null!;

    public static void Load()
    {
        Config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Settings/appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}