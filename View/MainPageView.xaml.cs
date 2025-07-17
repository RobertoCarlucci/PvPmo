namespace PvPmo.View;

public partial class MainPageView : ContentPage
{
    public MainPageView(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }    
}
