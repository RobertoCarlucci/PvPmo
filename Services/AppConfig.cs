using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace PvPmo.Services;

public static class AppConfig
{
    private static IConfigurationRoot _config;
    static AppConfig()
    {
        string env = GetEnvironmentName();
        string resourceName = $"PvPmo.Data.appsettings.{env}.json";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        _config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }
    public static string GetConnectionString(string name) =>
        _config.GetConnectionString(name);

    public static string GetSetting(string section, string key) =>
        _config.GetSection(section)[key];

    private static string GetEnvironmentName()
    {
#if DEV
        return "DEV";
#elif PROD
        return "PROD";  
#else
        return "DEFAULT";  
#endif
    }
}