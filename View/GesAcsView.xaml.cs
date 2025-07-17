namespace PvPmo.View;

public partial class GesAcsView : ContentPage
{
	public GesAcsView(GesAcsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}