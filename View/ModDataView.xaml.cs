namespace PvPmo.View;

public partial class ModDataView : ContentPage
{
	public ModDataView(ModDataViewModel viewModel)
	{
        InitializeComponent();
		BindingContext = viewModel;        
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}