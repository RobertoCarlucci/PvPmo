using PvPmo.View;

namespace PvPmo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(ModData), typeof(ModData));
            Routing.RegisterRoute(nameof(GesAcs), typeof(GesAcs));
        }
    }
}
