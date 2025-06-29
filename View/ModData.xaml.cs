using System.Diagnostics;

namespace PvPmo.View;

public partial class ModData : ContentPage
{
	public ModData(ModDataViewModel viewModel)
	{
        InitializeComponent();
		BindingContext = viewModel;        
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}