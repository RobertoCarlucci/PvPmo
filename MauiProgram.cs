using Microsoft.Extensions.Logging;
using PvPmo.View;

namespace PvPmo;

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
        builder.Services.AddSingleton<MainPageView>();
        builder.Services.AddSingleton<MainViewModel>();            
        builder.Services.AddTransient<GesAcsView>();
        builder.Services.AddTransient<GesAcsViewModel>();
        builder.Services.AddTransient<ModDataView>();
        builder.Services.AddTransient<ModDataViewModel>();       
        builder.Services.AddTransient<EditorConfigView>();
        builder.Services.AddSingleton<EditorConfigViewModel>();
        builder.Services.AddTransient<FileSetupConfigView>();
        builder.Services.AddTransient<FileSetupConfigViewModel>();

        //Services

        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ServizioAutenticazione>();
        builder.Services.AddSingleton<SagaService>();

        return builder.Build();
    }        
}
