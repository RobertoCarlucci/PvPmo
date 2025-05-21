namespace PvPmo.ViewModel;

public partial class DateRowViewModel : ObservableObject
{
    [ObservableProperty]
    int id;

    public Dictionary<string, string> Fields { get; set; } = new();
}
