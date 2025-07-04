using Microsoft.Extensions.Configuration;

namespace PvPmo.GestDb
{
    public static class AppConfig
    {
        private static IConfigurationRoot _config;
        static AppConfig()
        {
            string env = GetEnvironmentName();
            string fileName = $"appsettings.{env}.json";

            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(fileName, optional: false, reloadOnChange: true)
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
}


    //public static class AppConfig
    //{
    //    private static IConfigurationRoot _config;

    //    static AppConfig()
    //    {
    //        _config = new ConfigurationBuilder()
    //            .SetBasePath(AppContext.BaseDirectory)
    //            //.SetBasePath(FileSystem.AppDataDirectory) // o Directory.GetCurrentDirectory() per debug
    //            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //            .Build();
    //    }

    //    public static string GetConnectionString(string name) =>
    //        _config.GetConnectionString(name);

    //    public static string GetSetting(string section, string key) =>
    //        _config.GetSection(section)[key];
    //}


