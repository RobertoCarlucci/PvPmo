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
    private string key = string.Empty;

    [ObservableProperty]
    private string value = string.Empty;

    public bool IsValidDate => DateTime.TryParse(Value, out _);

    public DateTime ParsedDate
    {
        get
        {
            if (DateTime.TryParse(Value, out var date))
            {
                // restituisce sempre il primo giorno del mese
                return new DateTime(date.Year, date.Month, 1);
            }

            // evita problemi di binding, fallback visivo per DatePicker
            return DateTime.Now;
        }
        set
        {
            // memorizza la data in formato compatibile col DB
            Value = value.ToString("yyyy-MM-dd");
        }
    }
}