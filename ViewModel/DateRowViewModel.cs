using System.Collections.ObjectModel;

namespace PvPmo.ViewModel;

public partial class DateRowViewModel : BaseViewModel
{
    [ObservableProperty]
    int id;

    [ObservableProperty]
    ObservableCollection<FieldItem> fields = new();
}
public partial class FieldItem : ObservableObject
{
    [ObservableProperty]
    string key = string.Empty;

    [ObservableProperty]
    string value = string.Empty;
}
