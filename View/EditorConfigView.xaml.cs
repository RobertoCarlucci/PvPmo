namespace PvPmo.View;

public partial class EditorConfigView : ContentPage
{
	public EditorConfigView(JsonEditorViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}