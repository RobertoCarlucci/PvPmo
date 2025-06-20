using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PvPmo.ViewModel;
public partial class ModDataViewModel : BaseViewModel // ✅ FIX
{
    [ObservableProperty]
    ObservableCollection<DateRowViewModel> dateRows = new();

    public ModDataViewModel()
    {
        Title = "Modifica Date";
        _ = LoadAsync();
    }

    //public async Task LoadAsync()
    //{
    //    string strConn = await Conn.MysqlConn("pmo");
    //    var query = "SELECT * FROM `01_tabella_data`;";
    //    var table = new DataTable();
    //    await SqlAsync.SqlQryDataTable(strConn, query, table);

    //    foreach (DataRow row in table.Rows)
    //    {
    //        var dateRow = new DateRowViewModel();

    //        foreach (DataColumn col in table.Columns)
    //        {
    //            var columnName = col.ColumnName;

    //            if (columnName.Equals("id", StringComparison.OrdinalIgnoreCase))
    //            {
    //                if (int.TryParse(row[columnName]?.ToString(), out int idVal))
    //                    dateRow.Id = idVal;
    //                else
    //                    Debug.WriteLine("❌ ID non valido trovato nella riga.");
    //                continue;
    //            }

    //            //if (columnName.ToLower() == "id" && row[columnName] is int idVal)
    //            //{
    //            //    dateRow.Id = idVal;
    //            //    continue;
    //            //}

    //            var value = row[columnName]?.ToString() ?? string.Empty;
    //            dateRow.Fields.Add(new FieldItem { Key = columnName, Value = value });
    //        }

    //        DateRows.Add(dateRow);
    //    }
    //}

    public async Task LoadAsync()
    {
        string strConn = await Conn.MysqlConn("pmo");
        string query = "SELECT * FROM `01_tabella_data`;";
        var table = new DataTable();

        await SqlAsync.SqlQryDataTable(strConn, query, table);

        foreach (DataRow row in table.Rows)
        {
            var dateRow = new DateRowViewModel();

            foreach (DataColumn col in table.Columns)
            {
                string columnName = col.ColumnName;

                if (columnName.ToLower() == "id")
                {
                    if (int.TryParse(row[columnName]?.ToString(), out int idVal))
                        dateRow.Id = idVal;
                    continue;
                }

                string raw = row[columnName]?.ToString() ?? string.Empty;

                var field = new FieldItem
                {
                    Key = columnName,
                    Value = DateTime.TryParse(raw, out var dateVal)
                            ? dateVal.ToString("yyyy-MM-dd")
                            : string.Empty
                };

                dateRow.Fields.Add(field);
            }

            DateRows.Add(dateRow);
        }
    }


    [RelayCommand]
    public async Task BtnSalvaEsci()
    {
        string strConn = await Conn.MysqlConn("pmo");

        foreach (var row in DateRows)
        {
            var updates = row.Fields.Select(field =>
            {
                if (field.IsValidDate)
                {
                    var parsed = field.ParsedDate;
                    var firstOfMonth = new DateTime(parsed.Year, parsed.Month, 1);
                    string formatted = firstOfMonth.ToString("yyyy-MM-dd");
                    return $"`{field.Key}` = '{formatted}'";
                }
                else
                {
                    return $"`{field.Key}` = NULL";
                }
            });

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
