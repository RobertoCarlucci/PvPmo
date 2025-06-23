using System.Collections.ObjectModel;

namespace PvPmo.Util
{
    public partial class Utilita
    {
        public static DataTable ToDataTable<T>(ObservableCollection<T> collection)
        {
            DataTable table = new DataTable(typeof(T).Name);

            // Usa reflection per ottenere le proprietà pubbliche di T
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in collection)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
