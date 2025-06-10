namespace PvPmo.GestDb
{
    public partial class DbUtlil
    {
        // Classe per gestire le operazioni di utilità del database.
        
        public static List<MySqlParameter> Params = new List<MySqlParameter>();
        public static void AddParam(string nome, Object vale)
        {
            var NewParam = new MySqlParameter(nome, vale);
            Params.Add(NewParam);
        }
        
        private static List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
        public static void AddMapping(int sourceOrdinal, string destinationColumn)
        {
            var NewMapping = new MySqlBulkCopyColumnMapping(sourceOrdinal, destinationColumn);
            Mappings.Add(NewMapping);
        }
        
        // Vengono lette le intestazioni delle colonne Access e Sql per creare il mapping
        // delle corrispondenze tra le due e di conseguenza caricato attraverso il metodo
        // MyMapping e restituire al chiamante un oggetto mappings.
        public static async Task<List<MySqlBulkCopyColumnMapping>> MyMapping(string select,string dbDest, string tabDest, 
            string? dbOrgn = null, string? tbOrgn = null, string? nomeWorkSheet = null, string? filePath = null)
        {
            DataTable dTabOrgn = new();
            DataTable dTabDest = new();
            int dif = 0;

            if (select == "ACS" && !string.IsNullOrEmpty(tbOrgn) && !string.IsNullOrEmpty(filePath) && dbOrgn != null)
            {
                string qryAcs = $"SELECT * FROM [{tbOrgn}] WHERE 1=0;";
                string qrySql = $"SHOW COLUMNS FROM `{tabDest}`;";

                string conndbDest = Conn.MysqlConn(dbDest);
                string connOrgn = Conn.AcsDbConn(dbOrgn, filePath);

                await AcsAsync.AcsQryTab(connOrgn, qryAcs, dTabOrgn);
                await SqlAsync.SqlQryDataTable(conndbDest, qrySql, dTabDest, 30); // CORRETTO: usa DataTable non NoQry
            }
            else if (select == "EXL" && !string.IsNullOrEmpty(filePath))
            {
                string connOrgn = Conn.ExlFileConn(filePath);
                string conndbDest = Conn.MysqlConn(dbDest);

                string qryExl = $@"SELECT * FROM [{nomeWorkSheet}$] WHERE 1=0;";
                string qrySql = $@"SHOW COLUMNS FROM `{tabDest}`;";

                await ExlAsync.ExcQry(connOrgn, qryExl, dTabOrgn);
                await SqlAsync.SqlQryDataTable(conndbDest, qrySql, dTabDest, 30);
            }
            else if (select == "SQL" && !string.IsNullOrEmpty(dbOrgn))
            {
                string conndbDest = Conn.MysqlConn("pmo");
                string qryProd = $"SHOW COLUMNS FROM `{tabDest}`;";
                await SqlAsync.SqlQryDataTable(conndbDest, qryProd, dTabDest);

                string connOrgn = Conn.MysqlConn(dbOrgn);
                string qryUptd = $@"SHOW COLUMNS FROM `{tbOrgn}`;";
                await SqlAsync.SqlQryDataTable(connOrgn, qryUptd, dTabOrgn);

                dif = dTabDest.Rows.Count - dTabOrgn.Rows.Count;

                if (dif == 0)
                {
                    Mappings.Clear(); // reset mappings statici

                    for (int i = 0; i < dTabOrgn.Rows.Count && i < dTabDest.Rows.Count; i++)
                    {
                        if (dTabDest.Rows[i]["Field"].ToString() != "id")
                        {
                            string? destCol = dTabDest.Rows[i]["Field"]?.ToString();
                            AddMapping(i, destCol ?? $"Row{i}");
                        }
                    }
                    return Mappings;
                }
                // Colonne diverse > 1 → conferma da utente
                var confermaOk = await Shell.Current.DisplayAlert("Errore colonne",
                $"Le colonne nella tabella di origine: {dbOrgn} - ({dTabOrgn.Columns.Count}) e nella tabella di destinazione" +
                $": {dbDest} - ({dTabDest.Rows.Count}) non coincidono. Vuoi continuare con le altre tabelle?", "Si", "No");

                return Mappings;
            }

            dif = dTabDest.Rows.Count - dTabOrgn.Columns.Count;

            if (dif == 0 || dif == 1)
            {
                Mappings.Clear(); // reset mappings statici
                int offset = dif == 1 ? 1 : 0;

                for (int i = 0; i < dTabOrgn.Columns.Count && (i + offset) < dTabDest.Rows.Count; i++)
                {
                    string? destCol = dTabDest.Rows[i + offset]["Field"]?.ToString();
                    AddMapping(i, destCol ?? $"Col{i + offset}");
                }
                return Mappings;
            }
            // Colonne diverse > 1 → conferma da utente
            var conferma = await Shell.Current.DisplayAlert("Errore colonne",
                $"Le colonne nella tabella di origine: {dbOrgn} - ({dTabOrgn.Columns.Count}) e nella tabella di destinazione" +
                $": {dbDest} - ({dTabDest.Rows.Count}) non coincidono. Vuoi continuare con le altre tabelle?", "Si", "No");
            return Mappings;
        }
    }
}
public class SqlQry
{
    public static async Task<bool> CreaIdMonthYear(string conn, string tab, string col, string concat) =>
        await UpdtColConcat(conn, tab, col, concat);

    public static async Task<bool> CreaKeyId(string conn, string tab, string col, string concat) =>
        await UpdtColConcat(conn, tab, col, concat);

    public static async Task<bool> CreaDateId(string conn, string tab, string col, string concat) =>
        await UpdtColConcat(conn, tab, col, concat);

    private static async Task<bool> UpdtColConcat(string conn, string tab, string col, string concat)
    {
        string sql = $"UPDATE `{tab}` SET `{col}` = CONCAT({concat});";
        return await SqlAsync.SqlNoQry(conn, sql, 180);
    }

    public static async Task<bool> AddColSql(string conn, string tab, string col, string type)
    {
        string sql = $"ALTER TABLE `{tab}` ADD COLUMN `{col}` {type};";
        return await SqlAsync.SqlNoQry(conn, sql, 30);
    }

    public static async Task<bool> DelColSql(string conn, string tab, string col)
    {
        string sql = $"ALTER TABLE `{tab}` DROP COLUMN IF EXISTS `{col}`;";
        return await SqlAsync.SqlNoQry(conn, sql, 30);
    }

    public static async Task<bool> DelRecSql(string db, string tab)
    {
        string conn = Conn.MysqlConn(db);
        string sql = $"DELETE FROM `{tab}`;";
        return await SqlAsync.SqlNoQry(conn, sql, 30);
    }

    public static async Task<bool> RinColSql(string conn, string tab, string oldCol, string newCol, string type)
    {
        string sql = $"ALTER TABLE `{tab}` CHANGE `{oldCol}` `{newCol}` {type};";
        return await SqlAsync.SqlNoQry(conn, sql, 30);
    }

    public static async Task<DataTable> NomiColSql(string conn, string tab, DataTable schema)
    {
        string qry = $"SHOW COLUMNS FROM `{tab}`;";
        await SqlAsync.SqlQryDataTable(conn, qry, schema);
        return schema;
    }
}
public class ExlQry
{
    public static async Task<int> SelExlQry(string fileImp, string foglio)
    {
        string StrConn = Conn.ExlFileConn(fileImp);
        string qry = $"SELECT * FROM [{foglio}$];";
        int Num = await ExlAsync.ExcQry(qry, StrConn);
        return Num;
    }
    public static async Task<bool> SelExlQry(string fileImp, string foglio, DataTable tabImp)
    {
        string StrConn = Conn.ExlFileConn(fileImp);
        string qry = $"SELECT * FROM [{foglio}$];";
        bool Bol = await ExlAsync.ExcQry(qry, StrConn, tabImp);
        return Bol;
    }
}
