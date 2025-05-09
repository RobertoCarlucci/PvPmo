using Microsoft.Extensions.Logging;
using PvPmo.View;

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

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<ModData>();
            builder.Services.AddTransient<ModDataViewModel>();
            builder.Services.AddTransient<GesAcs>();
            builder.Services.AddTransient<GesAcsViewModel>();

            //Services

            builder.Services.AddPvPmoServices();

            return builder.Build();
        }
    }
}
