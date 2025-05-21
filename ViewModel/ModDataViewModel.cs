using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PvPmo.ViewModel;

public partial class ModDataViewModel : BaseViewModel
{
    [ObservableProperty]
    ObservableCollection<DateRowViewModel> dateRows = new();

    //public ObservableCollection<DateRowViewModel> DateRows { get; } = new();

public ModDataViewModel()
    {
        Title = "Modifica Date";
        _ = LoadAsync(); // Explicitly discard the task to suppress the warning  
    }
    public async Task LoadAsync()
    {
        string strConn = Conn.MysqlConn("pmo");
        string query = "SELECT * FROM `01_tabella_data`;";
        var table = new DataTable();

        await SqlAsync.SqlQryDataTable(strConn, query, table);

        foreach (DataRow row in table.Rows)
        {
            var dateRow = new DateRowViewModel();

            foreach (DataColumn col in table.Columns)
            {
                var columnName = col.ColumnName;

                if (columnName.ToLower() == "id" &&
                    row[columnName] is int idVal)
                {
                    dateRow.Id = idVal;
                    continue;
                }

                var value = row[columnName]?.ToString() ?? string.Empty;
                dateRow.Fields[columnName] = value;
            }

            DateRows.Add(dateRow);
            Debug.WriteLine($"DateRows count: {DateRows.Count}");
        }
    }

    [RelayCommand]
    public async Task BtnSalvaEsci()
    {
        string strConn = Conn.MysqlConn("pmo");

        foreach (var row in DateRows)
        {
            var updates = row.Fields
                .Select(kv => $"`{kv.Key}` = '{kv.Value.Replace("'", "''")}'");

            string sql = $"UPDATE `01_tabella_data` SET {string.Join(", ", updates)} WHERE id = {row.Id};";
            await SqlAsync.SqlNoQry(strConn, sql);
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task BtnAnnullaEsci()
    {
        await Shell.Current.GoToAsync("..");
    }
}
