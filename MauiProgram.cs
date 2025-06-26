using Microsoft.Extensions.Logging;
using PvPmo.View;
using System.Diagnostics;

namespace PvPmo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            //Wiews

            builder.Services.AddSingleton<LoginView>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();            
            builder.Services.AddTransient<GesAcs>();
            builder.Services.AddTransient<GesAcsViewModel>();
            builder.Services.AddTransient<ModData>();
            builder.Services.AddTransient<ModDataViewModel>();

            //Services

            builder.Services.AddPvPmoServices();

            EnsureLogFolderExists();

            return builder.Build();
        }
        private static void EnsureLogFolderExists()
        {
            var logPath = Path.Combine(AppContext.BaseDirectory, "Archivio", "Logs");
            try
            {
                Directory.CreateDirectory(logPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Errore creazione cartella log: {ex.Message}");
            }
        }
    }
}
