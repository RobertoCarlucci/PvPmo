namespace PvPmo.View;

public partial class GesAcs : ContentPage
{
	public GesAcs(GesAcsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}