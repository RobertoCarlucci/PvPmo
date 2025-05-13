namespace PvPmo.Util
{
    public partial class MappingExlSql
    {
        public static async Task<bool> CreaMappingExlSql(string strConnExl, string strConnSql, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
        {
            DataTable tabExl = new();
            DataTable tabSql = new();

            string qryExl = $"SELECT * FROM [{nomeWorkSheet}$] WHERE 1=0;";
            string qrySql = $"SHOW COLUMNS FROM `{nomeTbSql}`;";

            await ExlAsync.ExcQry(strConnExl, qryExl, tabExl);
            await SqlAsync.SqlQryDataTable(strConnSql, qrySql, tabSql, 30);

            int dif = tabSql.Rows.Count - tabExl.Columns.Count;

            if (dif == 0 || dif == 1)
            {
                SqlAsync.Mappings.Clear(); // Reset mapping statico

                int offset = dif == 1 ? 1 : 0;

                for (int i = 0; i < tabExl.Columns.Count && (i + offset) < tabSql.Rows.Count; i++)
                {
                    string? destCol = tabSql.Rows[i + offset]["Field"]?.ToString();
                    SqlAsync.AddMapping(i, destCol ?? $"Col{i + offset}");
                }

                return true;
            }
            // Differenza colonne > 1 → alert utente
            var conferma = await Shell.Current.DisplayAlert(
                "Errore colonne",
                $"Excel: {tabExl.Columns.Count} col.\nMySQL: {tabSql.Rows.Count} col.\nVuoi continuare con le altre tabelle?",
                "Si", "No");

            return conferma;
        }
    }
}
