namespace PvPmo.View;

public partial class ModDataView : ContentPage
{
    private readonly ModDataViewModel _viewModel;

    public ModDataView(ModDataViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
