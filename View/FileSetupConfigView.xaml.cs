namespace PvPmo.View;

public partial class FileSetupConfigView : ContentPage
{
    public FileSetupConfigView(FileSetupConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
