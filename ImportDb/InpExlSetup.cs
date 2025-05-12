namespace PvPmo.ImportDb
{
    public partial class InpExlSetup
    
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


    //    public static async Task<bool> CreaMappingExlSql(string StrConnExl, string StrConnSql, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
    //    {
    //        DataTable _tabExl = new DataTable();
    //        DataTable _tabSql = new DataTable();
    //        string QryExl = "SELECT * FROM [" + nomeWorkSheet + "$] WHERE 1=0;";
    //        string QrySql = "SHOW COLUMNS FROM `" + nomeTbSql + "`;";

    //        ExlSync.ExcQry(StrConnExl, QryExl, _tabExl);

    //        // Replace the call to the non-existent SqlQryDataReader with a valid alternative
    //        bool queryResult = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30);
    //        if (queryResult)
    //        {
    //            // Assuming the result of the query is stored in _tabSql
    //            // You may need to implement logic to populate _tabSql from the query result
    //        }

    //        int _dif = _tabSql.Rows.Count - _tabExl.Columns.Count;

    //        if (_dif == 0)
    //        {
    //            DataRow[] _rowSql = _tabExl.Select();
    //            for (int x = 1; x < _tabSql.Rows.Count; x++)
    //            {
    //                int SourceOrdinal = x;
    //                string? DestinationColumn = _tabSql.Rows[x]["Field"].ToString();
    //                SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
    //            }
    //            return true;
    //        }
    //        else if (_dif == 1)
    //        {
    //            DataRow[] _rowSql = _tabExl.Select();
    //            for (int x = 1; x < _tabSql.Rows.Count; x++)
    //            {
    //                int SourceOrdinal = x - 1;
    //                string? DestinationColumn = _tabSql.Rows[x]["Field"].ToString();
    //                SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
    //            }
    //            return true;
    //        }
    //        else if (_dif < 0 || _dif > 1)
    //        {
    //            var Conf = await Shell.Current.DisplayAlert
    //                ("Errore nel numero di Colonne delle Tabelle.", "La Tabella Excel: "
    //                + _tabExl + " Ha un numero di colonne diverso dalla tabella Sql: "
    //                + _tabSql + " Vuoi continuare ad importare le altre tabelle residue ? "
    //                , "Si", "No");
    //            if (Conf == true)
    //                return true;
    //            else
    //                return false;
    //        }
    //        return false;
    //    }
    }
}
