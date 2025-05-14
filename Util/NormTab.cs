using System.Text;

namespace PvPmo.Util
{
    public static class NormTab
    {
        // Viene caricata la tabella "normalizza" dal Db "pvpmo_origine"
        //        // utilizzando dalla cartella Service il servizio "CaricaTabNorm" che mi
        //        // fornisce la mappatura delle colonne da modificare e se necessario anche la
        //        // stringa da inserire nel comando Sql CONCAT

        public static async Task<bool> NormTabImp(string tipoImportazione)
        {
            // Carico la tabella con le azioni da svolgere dal Service
            var repo = new CaricaTabRepository<CaricaTabNorm>("pvpmo_origine", "normalizza");
            var dati = await repo.GetAllAsync();

            bool esitoGlobale = true;

            foreach (var n in dati)
            {
                // "tipoImportazione" identifica il tipo di file da lavorare ACS o EXL
                if (n.InpType != tipoImportazione)
                    continue;

                string connStr = Conn.MysqlConn(n.DbDest);

                bool esitoSingolo = n.Azione switch
                {
                    "DEL" => await SqlQry.DelColSql(connStr, n.TabellaMod, n.ColDaMod),
                    "REN" => await SqlQry.RinColSql(connStr, n.TabellaMod, n.ColDaMod, n.Modifica, n.TipoCol),
                    "ADD" => await SqlQry.AddColSql(connStr, n.TabellaMod, n.ColDaMod, n.TipoCol),
                    "GEN" => await EseguiGenerazione(connStr, n),
                    _ => true
                };
                esitoGlobale &= esitoSingolo;
            }
            return esitoGlobale;
        }
        private static async Task<bool> EseguiGenerazione(string conn, CaricaTabNorm n) => n.ColDaMod switch
        {
            "Dateid" => await SqlQry.CreaDateId(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
            "IdMonthYear" => await SqlQry.CreaIdMonthYear(conn, n.TabellaMod, n.ColDaMod, n.Modifica),
            "Keyid" => await SqlQry.CreaKeyId(conn, n.TabellaMod, n.ColDaMod, n.Modifica),_ => true
        };

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
            var conferma = await Shell.Current.DisplayAlert("Errore colonne", 
                $"Le colonne in Access ({tabAcs.Columns.Count}) e in SQL ({tabSql.Rows.Count}) non coincidono.\nVuoi continuare con le altre tabelle?",
                "Si", "No");

            return conferma;
        }

        // uguale alla precedente con la differenza che vengono lette le intestazioni delle colonne
        // Excel e Sql per creare il mapping.
        public static async Task<bool> CreaMappingExlSql(string strConnExl, string strConnSql, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
        {
            DataTable tabExl = new();
            DataTable tabSql = new();

            string qryExl = $"SELECT * FROM [{nomeWorkSheet}$] WHERE 1=0;";
            string qrySql = $"SHOW COLUMNS FROM `{nomeTbSql}`;";

            await ExlAsync.ExcQry(strConnExl, qryExl, tabExl);
            await SqlAsync.SqlQryDataTable(strConnSql, qrySql, tabSql, 30);

            int dif = tabSql.Rows.Count - tabExl.Columns.Count;
            //int totcolexl = tabExl.Columns.Count;

            if (dif == 0 || dif == 1)
            {
                SqlAsync.Mappings.Clear(); // Reset mapping statico

                int offset = dif == 1 ? 1 : 0;

                for (int i = 0; i < tabExl.Columns.Count && (i + offset) < tabSql.Rows.Count; i++)
                //for (int i = 0; i < totcolexl + offset; i++)
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
            var simboliDaRimuovere = new[] { ",", "-", "_", " ", "(", ")", "#" };
            foreach (var simbolo in simboliDaRimuovere)
                name = name.Replace(simbolo, "");
            return name;
        }
    }    
}
