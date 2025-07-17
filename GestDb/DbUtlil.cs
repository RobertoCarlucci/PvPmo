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
            string? dbOrgn = null, string? tbOrgn = null, string? nomeWorkSheet = null, string? fullPath = null)
        {
            DataTable dTabOrgn = new();
            DataTable dTabDest = new();
            int dif = 0;            

            if (select == "ACS" && !string.IsNullOrEmpty(tbOrgn) && !string.IsNullOrEmpty(fullPath) && dbOrgn != null)
            {
                string _conndb = string.Empty;
                string _connAcs = string.Empty;

                _conndb = (!string.IsNullOrEmpty(dbDest)) ? _conndb = await Conn.MysqlConn(dbDest) : _conndb;
                if (string.IsNullOrEmpty(_conndb))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                    Mappings.Clear();
                    return Mappings;
                }

                _connAcs = (!string.IsNullOrEmpty(dbOrgn) || !string.IsNullOrEmpty(fullPath)) ? _connAcs = await Conn.AcsDbConn(dbOrgn, fullPath) : _connAcs;
                if (string.IsNullOrEmpty(_connAcs))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                    Mappings.Clear();
                    return Mappings;
                }

                string qryAcs = $"SELECT * FROM [{tbOrgn}] WHERE 1=0;";
                string qrySql = $"SHOW COLUMNS FROM `{tabDest}`;";                

                await AcsAsync.AcsQryTab(_connAcs, qryAcs, dTabOrgn);
                await SqlAsync.SqlQryDataTable(_conndb, qrySql, dTabDest, 30); // CORRETTO: usa DataTable non NoQry
            }
            else if (select == "EXL")
            {
                string _connExl = string.Empty;
                string _conndb = string.Empty;

                _conndb = (!string.IsNullOrEmpty(dbDest)) ? _conndb = await Conn.MysqlConn(dbDest) : _conndb;
                if (string.IsNullOrEmpty(_conndb))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                    Mappings.Clear();
                    return Mappings;
                }

                _connExl = (!string.IsNullOrEmpty(fullPath)) ? _connExl = await Conn.ExlFileConn(fullPath) : _connExl;
                if (string.IsNullOrEmpty(_connExl))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al File.", "OK");
                    Mappings.Clear();
                    return Mappings;
                }

                string qryExl = $@"SELECT * FROM [{nomeWorkSheet}$] WHERE 1=0;";
                string qrySql = $@"SHOW COLUMNS FROM `{tabDest}`;";

                await ExlAsync.ExcQry(_connExl, qryExl, dTabOrgn);
                await SqlAsync.SqlQryDataTable(_conndb, qrySql, dTabDest, 30);
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
        private static Dictionary<string, List<MySqlBulkCopyColumnMapping>> mapsDict = new();
        public static void AddListMapping(string nome, int sourceOrdinal, string destinationColumn)
        {
            // Fix for CS0029: Correctly add the mapping to the dictionary instead of assigning a List to a string variable
            if (mapsDict == null) { mapsDict = new Dictionary<string, List<MySqlBulkCopyColumnMapping>>(); }

            if (!mapsDict.ContainsKey(nome)) { mapsDict[nome] = new List<MySqlBulkCopyColumnMapping>(); }

            mapsDict[nome].Add(new MySqlBulkCopyColumnMapping
            {
                SourceOrdinal = sourceOrdinal,
                DestinationColumn = destinationColumn
            });
        }
        public static List<MySqlBulkCopyColumnMapping> ApplicaMappingList(MySqlBulkCopy bulkCopy, string nomeMapping)
        {
            if (!mapsDict.ContainsKey(nomeMapping))
            {
                throw new ArgumentException($"Profilo di mapping '{nomeMapping}' non trovato.", nameof(nomeMapping));
            }

            var mappingDaApplicare = mapsDict[nomeMapping];

            // FONDAMENTALE: Pulisci sempre i mapping precedenti prima di aggiungerne di nuovi
            bulkCopy.ColumnMappings.Clear();

            // Aggiungi i mapping del profilo scelto
            foreach (var mapping in mappingDaApplicare)
            {
                bulkCopy.ColumnMappings.Add(mapping);
            }
            //Console.WriteLine($"Applicato il profilo di mapping: '{nomeMapping}'");
            return mappingDaApplicare;
        }

        public static async Task<Dictionary<string, List<MySqlBulkCopyColumnMapping>>> MyMappingList(
            string nMapping, string connProd, string tabDest, string connUptd, string tbOrgn)
        {
            DataTable dTabOrgn = new();
            DataTable dTabDest = new();           

            string _connProd = await Conn.MysqlConn(connProd);
            string _connUptd = await Conn.MysqlConn(connUptd);

            if (string.IsNullOrEmpty(_connProd) || string.IsNullOrEmpty(_connUptd))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                mapsDict.Clear();
                return mapsDict;
            }
            
            string qryProd = $"SHOW COLUMNS FROM `{tabDest}`;";
            await SqlAsync.SqlQryDataTable(_connProd, qryProd, dTabDest);

            string qryUptd = $@"SHOW COLUMNS FROM `{tbOrgn}`;";
            await SqlAsync.SqlQryDataTable(_connUptd, qryUptd, dTabOrgn);

            int dif = dTabDest.Rows.Count - dTabOrgn.Rows.Count;

            if (dif == 0)
            {
                for (int i = 0; i < dTabOrgn.Rows.Count && i < dTabDest.Rows.Count; i++)
                {
                    if (dTabDest.Rows[i]["Field"].ToString() != "id")
                    {
                        string? destCol = dTabDest.Rows[i]["Field"]?.ToString();
                        AddListMapping(nMapping, i, destCol ?? $"Row{i}");
                    }
                }
                return mapsDict;
            }

            var confermaOk = await Shell.Current.DisplayAlert("Errore colonne",
                $"Le colonne nella tabella di origine: {connUptd} - ({dTabOrgn.Columns.Count}) e nella tabella di destinazione" +
                $": {connProd} - ({dTabDest.Rows.Count}) non coincidono. Vuoi continuare con le altre tabelle?", "Si", "No");

            return mapsDict;
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
        return await SqlAsync.SqlNoQryString(conn, sql, 180);
    }
    public static async Task<bool> AddColSql(string conn, string tab, string col, string type)
    {
        string sql = $"ALTER TABLE `{tab}` ADD COLUMN `{col}` {type};";
        return await SqlAsync.SqlNoQryString(conn, sql, 30);
    }
    public static async Task<bool> DelColSql(string conn, string tab, string col)
    {
        string sql = $"ALTER TABLE `{tab}` DROP COLUMN IF EXISTS `{col}`;";
        return await SqlAsync.SqlNoQryString(conn, sql, 30);
    }
    public static async Task<bool> DelRecSql(string db, string tab)
    {
        string conn = await Conn.MysqlConn(db);
        string sql = $"DELETE FROM `{tab}`;";
        return await SqlAsync.SqlNoQryString(conn, sql, 30);
    }
    public static async Task<bool> RinColSql(string conn, string tab, string oldCol, string newCol, string type)
    {
        string sql = $"ALTER TABLE `{tab}` CHANGE `{oldCol}` `{newCol}` {type};";
        return await SqlAsync.SqlNoQryString(conn, sql, 30);
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
        string StrConn = await Conn.ExlFileConn(fileImp);
        string qry = $"SELECT * FROM [{foglio}$];";
        int Num = await ExlAsync.ExcQry(qry, StrConn);
        return Num;
    }
    public static async Task<bool> SelExlQry(string fileImp, string foglio, DataTable tabImp)
    {
        string StrConn = await Conn.ExlFileConn(fileImp);
        string qry = $"SELECT * FROM [{foglio}$];";
        bool Bol = await ExlAsync.ExcQry(qry, StrConn, tabImp);
        return Bol;
    }
}