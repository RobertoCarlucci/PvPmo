using PvPmo.View;

namespace PvPmo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
        Routing.RegisterRoute(nameof(MainPageView), typeof(MainPageView));
        Routing.RegisterRoute(nameof(ModDataView), typeof(ModDataView));
        Routing.RegisterRoute(nameof(GesAcsView), typeof(GesAcsView));       
        Routing.RegisterRoute(nameof(EditorConfigView), typeof(EditorConfigView));
        Routing.RegisterRoute(nameof(FileSetupConfigView), typeof(FileSetupConfigView));
    }
}
