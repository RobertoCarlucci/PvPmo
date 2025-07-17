namespace PvPmo.View;

public partial class JsonEditorView : ContentPage
{
	public JsonEditorView(JsonEditorViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}