using System.Text;

namespace PvPmo.Import;

public static class NormTab
{
    // Viene caricata la tabella "normalizza" dal Db "pvpmo_origine"
    // utilizzando dalla cartella Service il servizio "CaricaTabNorm" che mi
    // fornisce la mappatura delle colonne da modificare e se necessario anche la
    // stringa da inserire nel comando Sql CONCAT

    public static async Task<bool> NormTabImp(string tipoImportazione, string dblavoro)
    {            
        // Carico la tabella con le azioni da svolgere dal Service
        var databaseService = ServiceHelper.GetService<DatabaseService>();
        var dati = await databaseService.GetNormalizzaConfigsAsync();

        bool esitoGlobale = true;

        foreach (var n in dati)
        {
            // "tipoImportazione" identifica il tipo di file da lavorare ACS o EXL
            if (n.InpType != tipoImportazione && n.DbDest != dblavoro)
                continue;

            // Verifica che n.DbDest non sia null prima di passarlo a Conn.MysqlConn
            if (string.IsNullOrEmpty(n.DbDest))
            {
                esitoGlobale = false;
                continue;
            }

            string _connDb = string.Empty;

            _connDb = (!string.IsNullOrEmpty(n.DbDest)) ? _connDb = await Conn.MysqlConn(n.DbDest) : _connDb;
            if (string.IsNullOrEmpty(_connDb))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }

            // Verifica che n.TabellaMod e n.ColDaMod non siano null prima di passare i valori
            bool esitoSingolo = n.Azione switch
            {
                "DEL" => !string.IsNullOrEmpty(n.TabellaMod) && !string.IsNullOrEmpty(n.ColDaMod)
                    ? await SqlQry.DelColSql(_connDb, n.TabellaMod, n.ColDaMod)
                    : false,
                "REN" => !string.IsNullOrEmpty(n.TabellaMod) && !string.IsNullOrEmpty(n.ColDaMod) && !string.IsNullOrEmpty(n.Modifica) && !string.IsNullOrEmpty(n.TipoCol)
                    ? await SqlQry.RinColSql(_connDb, n.TabellaMod, n.ColDaMod, n.Modifica, n.TipoCol)
                    : false,
                "ADD" => !string.IsNullOrEmpty(n.TabellaMod) && !string.IsNullOrEmpty(n.ColDaMod) && !string.IsNullOrEmpty(n.TipoCol)
                    ? await SqlQry.AddColSql(_connDb, n.TabellaMod, n.ColDaMod, n.TipoCol)
                    : false,
                "GEN" => await EseguiGenerazione(_connDb, n),
                _ => true
            };
            esitoGlobale &= esitoSingolo;
        }
        return esitoGlobale;
    }
    private static async Task<bool> EseguiGenerazione(string conn, NormalizzaConfig n) =>
        !string.IsNullOrEmpty(n.TabellaMod) && !string.IsNullOrEmpty(n.Modifica) ? n.ColDaMod switch
        {
            "Dateid" => await SqlQry.CreaDateId(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
            "IdMonthYear" => await SqlQry.CreaIdMonthYear(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
            "Keyid" => await SqlQry.CreaKeyId(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
            _ => true
        } : false;

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
            if (originalName == "Divisore" || originalName == "Hours Per Week"
                || originalName == "Timesheet_Time" || originalName == "ORE")
                columnType = "FLOAT";
            if (originalName == "Modifica" || originalName == "TipoCol")
                columnType = "NVARCHAR(120)";
            if (originalName == "Res_Start_Date" || originalName == "Res_Finish_Date")
                columnType = "DATETIME";
            if (originalName == "Disapproved Timesheets" || originalName == "Overdue Timesheets"
                || originalName == "Resource Depth" || originalName == "Resource Quantity")
                columnType = "DOUBLE";

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
        "DateTime" => "DATE",
        "Boolean" => "TINYINT(1)",
        "Byte[]" => "BLOB",
        "TimeSpan" => "TIME",
        _ => "NVARCHAR(255)" // fallback
    };
    private static string NormalizzaNome(string name)
    {
        var simboliDaRimuovere = new[] {".", ",", "-", "_", " ", "(", ")", "#" };
        foreach (var simbolo in simboliDaRimuovere)
            name = name.Replace(simbolo, "");
        return name;
    }
}    
