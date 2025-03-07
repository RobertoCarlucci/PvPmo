using PvPmo.View;

namespace PvPmo
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ModData), typeof(ModData));
        }
    }
}
