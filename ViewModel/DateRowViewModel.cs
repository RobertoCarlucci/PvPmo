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
    public string Key { get; set; } = string.Empty;

    [ObservableProperty]
    private string value = string.Empty;

    public bool IsValidDate =>
        DateTime.TryParse(Value, out _);

    public DateTime ParsedDate
    {
        get => DateTime.TryParse(Value, out var date) ? date : DateTime.MinValue;
        set => Value = value.ToString("yyyy-MM-dd");
    }
}





