namespace PvPmo.View;

public partial class ModData : ContentPage
{
	public ModData(ModDataViewModel ViewModel)
	{
		InitializeComponent();
		BindingContext = ViewModel;
	}
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}