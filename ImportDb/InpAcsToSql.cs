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
        string Qry = $"SELECT * FROM [{nomeTbAcs}];";
        //string Qry = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0";
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

            // Se manca colonna ID, la aggiungiamo in cima
            // Se la colonna è ID e non è la prima colonna, non la consideriamo
            
            if (colIndex == 0 && !originalName.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                if (hasPrimaryKey == false)
                {
                    sb.Append("`id` INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT, ");
                    hasPrimaryKey = true;
                }                
            }            
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
                if (hasPrimaryKey == false)
                {
                    columnType = "INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT";
                    hasPrimaryKey = true;
                }                
            }
            // Override tipo per colonne specifiche
            if (originalName == "Divisore" || originalName == ("Hours Per Week")
                || originalName == ("Timesheet_Time") || originalName == ("ORE"))
                columnType = "FLOAT";
            if (originalName == "Modifica" || originalName == "TipoCol")
                columnType = "NVARCHAR(120)";

            sb.Append($"`{renamed}` {columnType}");

            if (colIndex < lastIndex)
            {
                sb.Append(", ");
            }
            else 
            {
                sb.Append("");
            }
            colIndex++;
                           
        }
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