using PvPmo.View;
using System.Collections.ObjectModel;

namespace PvPmo.ViewModel
{
    public partial class ModDataViewModel : BaseViewModel
    {
        [ObservableProperty]
        ObservableCollection<DateRowViewModel> dateRows = new();

        public ModDataViewModel()
        {
            Title = "Modifica Date";
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            string dbProd = "pmo";
            string strConn = await Conn.MysqlConn(dbProd);

            string query = "SELECT * FROM `01_tabella_data`;";
            var table = new DataTable();

            await SqlAsync.SqlQryDataTable(strConn, query, table);

            foreach (DataRow row in table.Rows)
            {
                var dateRow = new DateRowViewModel();

                foreach (DataColumn col in table.Columns)
                {
                    var columnName = col.ColumnName;

                    if (columnName.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(row[columnName]?.ToString(), out int idVal))
                            dateRow.Id = idVal;
                        continue;
                    }

                    var value = row[columnName]?.ToString() ?? string.Empty;

                    dateRow.Fields.Add(new FieldItem
                    {
                        Key = columnName,
                        Value = value
                    });
                }
                DateRows.Add(dateRow);
            }
        }

        [RelayCommand]
        public async Task BtnSalvaEsci()
        {
            string dbProd = "pmo";

            string strConn = await Conn.MysqlConn(dbProd);

            foreach (var row in DateRows)
            {
                var updates = row.Fields.Select(field =>
                {
                    string valueSql;

                    if (field.IsValidDate)
                    {
                        var parsed = field.ParsedDate;
                        var firstOfMonth = new DateTime(parsed.Year, parsed.Month, 1);
                        valueSql = $"'{firstOfMonth:yyyy-MM-dd}'";
                    }
                    else
                    {
                        valueSql = "NULL";
                    }

                    return $"`{field.Key}` = {valueSql}";
                });

                string sql = $"UPDATE `01_tabella_data` SET {string.Join(", ", updates)} WHERE id = {row.Id};";
                await SqlAsync.SqlNoQryString(strConn, sql);
            }
            await TestDate.VerificaDateDaTabellaAsync(strConn, "01_tabella_data");

            await Shell.Current.GoToAsync(nameof(MainPageView));
        }

        [RelayCommand]
        public async Task BtnAnnullaEsci()
        {
            await Shell.Current.GoToAsync(nameof(MainPageView));
        }
    }
}

