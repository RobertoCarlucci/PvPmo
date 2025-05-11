using System.Text;

namespace PvPmo.ImportDb;

public partial class InpAcsToSql
{
    //Importazione dei due Db Access con i loro dati.
    public static async Task NewDb()
    {
        string? _acsPath = await SelCart.PickFolder();
        if (!string.IsNullOrEmpty(_acsPath))
        {
            var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
            var dati = await repo.GetAllAsync();

            foreach (var n in dati)
            {
                if (n.InpType != "ACS") continue;

                if (string.IsNullOrWhiteSpace(n.DbInp) || string.IsNullOrWhiteSpace(n.Tabella) ||
                    string.IsNullOrWhiteSpace(n.DbDest) || string.IsNullOrWhiteSpace(n.TabellaSql))
                    continue;
                // Aggiunto per evitare che un errore blocchi tutte le tabelle:
                try
                {
                    bool ok1 = await TabAcstoTabSql(n.DbInp, n.Tabella, n.DbDest, n.TabellaSql, _acsPath);
                    bool ok2 = await InpAcsDatitoSql(n.DbInp, n.Tabella, n.DbDest, n.TabellaSql, _acsPath);

                    if (!ok1 || !ok2)
                    {
                        await Shell.Current.DisplayAlert("Errore", $"Errore durante l'import di {n.Tabella}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Errore", $"Tabella {n.Tabella}: {ex.Message}", "OK");
                }
            }
        }        
        bool norm = await NormTab.NormTabImp("ACS");
        if (!norm)
            await Shell.Current.DisplayAlert("Errore", "Normalizzazione fallita", "OK");
    }

    // Creo le tabelle Sql leggendo i nomi delle tabelle Access e normalizzando le
    // intestazioni delle colonne in modo compatibile con sql.
    public static async Task<bool> TabAcstoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        DataTable _tabella = new DataTable();
        string Qry = $"SELECT * FROM [{nomeTbAcs}$] WHERE 1=0;";
        await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
        Qry = NormInp(nomeTbSql, _tabella);
        Qry = "CREATE OR REPLACE TABLE " + Qry;
        string StrConnSql = Conn.MysqlConn(nomeDbSql);
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry, 30);
        return Bol;
    }
    // Importo i dati all'interno del Db andando a popolare con i valori le tabelle
    // colonne precedentemente create.

    public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        bool mappingOk = await CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, acsPath);

        if (!mappingOk)
            return false;

        string connAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        string connSql = Conn.MysqlConn(nomeDbSql);

        DataTable tabella = new();
        string qry = $"SELECT * FROM [{nomeTbAcs}]";

        await AcsAsync.AcsQryTab(connAcs, qry, tabella);
        return await SqlAsync.SqlBulkCopy(connSql, nomeTbSql, tabella, 180);
    }

    //public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    //{
    //    bool Bol = await CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, acsPath);
    //    if (Bol == true)
    //    {
    //        string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
    //        DataTable _tabella = new DataTable();
    //        string Qry = "SELECT * FROM [" + nomeTbAcs + "]";
    //        await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
    //        string StrConnSql = Conn.MysqlConn(nomeDbSql);
    //        await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella, 180);            
    //    }
    //    return Bol;
    //}

    // Vengono lette le intestazioni delle colonne Access e Sql per creare il mapping
    // delle corrispondenze tra le due e di conseguenza caricato attraverso il metodo
    // AddMapping all'interno della cartella GestDb classe gestione Sql.
    public static async Task<bool> CreaMappingAcsSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    {
        DataTable tabAcs = new();
        DataTable tabSql = new();

        string qryAcs = $"SELECT * FROM [{nomeTbAcs}] WHERE 1=0;";
        string qrySql = $"SHOW COLUMNS FROM `{nomeTbSql}`;";

        string connAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
        string connSql = Conn.MysqlConn(nomeDbSql);

        await AcsAsync.AcsQryTab(connAcs, qryAcs, tabAcs);
        await SqlAsync.SqlQryDataTable(connSql, qrySql, tabSql, 30); // CORRETTO: usa DataTable non NoQry

        int dif = tabSql.Rows.Count - tabAcs.Columns.Count;

        if (dif == 0 || dif == 1)
        {
            SqlAsync.Mappings.Clear(); // reset mappings statici
            int offset = dif == 1 ? 1 : 0;

            for (int i = 0; i < tabAcs.Columns.Count && (i + offset) < tabSql.Rows.Count; i++)
            {
                string? destCol = tabSql.Rows[i + offset]["Field"]?.ToString();
                SqlAsync.AddMapping(i, destCol ?? $"Col{i + offset}");
            }
            return true;
        }
        // Colonne diverse > 1 → conferma da utente
        var conferma = await Shell.Current.DisplayAlert(
            "Errore colonne",
            $"Le colonne in Access ({tabAcs.Columns.Count}) e in SQL ({tabSql.Rows.Count}) non coincidono.\nVuoi continuare con le altre tabelle?",
            "Si", "No");

        return conferma;
    }


    //public static async Task<bool> CreaMappingAcsSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
    //{
    //    DataTable _tabAcs = new DataTable();
    //    DataTable _tabSql = new DataTable();
    //    string QryAcs = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0;";
    //    string QrySql = "SHOW COLUMNS FROM `" + nomeTbSql + "`;";

    //    string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
    //    string StrConnSql = Conn.MysqlConn(nomeDbSql);

    //    await AcsAsync.AcsQryTab(StrConnAcs, QryAcs, _tabAcs);
    //    QrySql = "SHOW COLUMNS FROM `" + nomeTbSql + "`;";
    //    List<MySqlParameter> parameters = new List<MySqlParameter>(); // Se necessario, aggiungi parametri qui
    //    await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30, parameters);

    //    int _dif = _tabSql.Rows.Count - _tabAcs.Columns.Count;

    //    if (_dif == 0)
    //    {
    //        DataRow[] _rowSql = _tabAcs.Select();
    //        for (int x = 1; x < _tabSql.Rows.Count; x++)
    //        {
    //            int SourceOrdinal = x;
    //            string? DestinationColumn = _tabSql.Rows[x]["Field"].ToString();
    //            SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
    //        }
    //        return true;
    //    }
    //    else if (_dif == 1)
    //    {
    //        DataRow[] _rowSql = _tabAcs.Select();
    //        for (int x = 1; x < _tabSql.Rows.Count; x++)
    //        {
    //            int SourceOrdinal = x - 1;
    //            string? DestinationColumn = _tabSql.Rows[x]["Field"].ToString();
    //            SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
    //        }
    //        return true;
    //    }
    //    else if (_dif < 0 || _dif > 1)
    //    {
    //        var Conf = await Shell.Current.DisplayAlert
    //            ("Errore nel numero di Colonne delle Tabelle.", "La Tabella Access: "
    //            + _tabAcs + " Ha un numero di colonne diverso dalla tabella Sql: "
    //            + _tabSql + " Vuoi continuare ad importare le altre tabelle residue ? "
    //            , "Si", "No");
    //        if (Conf == true)
    //            return true;
    //        else

    //            return false;
    //    }
    //    return false;
    //}
    // Vengono normalizzati i nomi delle Tabelle Sql creando le stesse.
    // Si procede anche alla normalizzazione dei nomi colonna.
    public static string NormInp(string nomeTbSql, DataTable tabData)
    {
        var sb = new StringBuilder();
        sb.Append($"`{nomeTbSql}` (");

        bool hasPrimaryKey = false;
        int colIndex = 0;
        int lastIndex = tabData.Columns.Count - 1;

        foreach (DataColumn col in tabData.Columns)
        {
            string originalName = col.ColumnName;
            string columnType = MappaTipo(col.DataType.Name);

            // Rinomina nomi specifici
            string renamed = originalName switch
            {
                "ID" => "id",
                "Date" or "DateID" or "Date ID" => "Dateid",
                "Key" or "KeyID" => "Keyid",
                "ID&Month&Year" => "IdMonthYear",
                "Work ID #" => "WorkId",
                "Action Items, Completed (#)" => "ActionItemsCompletedVal",
                "Action Items, Completed (%)" => "ActionItemsCompletedPerc",
                "KeyFTE_Mese" => "KeyFteMese",
                "ORE" => "Ore",
                _ => originalName
            };

            renamed = NormalizzaNome(renamed);

            if (renamed.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                columnType = "INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT";
                hasPrimaryKey = true;
            }
            // Override tipo per colonne specifiche
            if (originalName == "Divisore" || originalName.Contains("Hours") || originalName.Contains("Time"))
                columnType = "FLOAT";
            if (originalName == "Modifica" || originalName == "TipoCol")
                columnType = "NVARCHAR(120)";

            sb.Append($"`{renamed}` {columnType}");

            if (colIndex < lastIndex)
                sb.Append(", ");

            colIndex++;
        }
        // Se manca colonna ID, la aggiungiamo in cima
        if (!hasPrimaryKey)
            sb.Insert(0, "`id` INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT, ");

        sb.Append(");");
        return sb.ToString();
    }
    private static string MappaTipo(string type) => type switch
    {
        "String" => "NVARCHAR(50)",
        "Int16" => "SMALLINT UNSIGNED",
        "Int32" => "INT UNSIGNED",
        "Int64" => "BIGINT UNSIGNED",
        "Single" => "FLOAT",
        "Double" => "DOUBLE",
        "Decimal" => "DECIMAL(18,2)",
        "DateTime" => "DATETIME",
        "Boolean" => "TINYINT(1)",
        "Byte[]" => "BLOB",
        "TimeSpan" => "TIME",
        _ => "NVARCHAR(255)" // fallback
    };
    private static string NormalizzaNome(string name)
    {
        var simboliDaRimuovere = new[] { ",", "-", "_", " ", "(", ")" };
        foreach (var simbolo in simboliDaRimuovere)
            name = name.Replace(simbolo, "");
        return name;
    }
}