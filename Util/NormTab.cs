using DocumentFormat.OpenXml.InkML;
using System.Text;

namespace PvPmo.Util
{
    public static class NormTab
    {
        private static List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
        public static void AddMapping(int sourceOrdinal, string destinationColumn)
        {
            var NewMapping = new MySqlBulkCopyColumnMapping(sourceOrdinal, destinationColumn);
            Mappings.Add(NewMapping);
        }

        // Viene caricata la tabella "normalizza" dal Db "pvpmo_origine"
        //        // utilizzando dalla cartella Service il servizio "CaricaTabNorm" che mi
        //        // fornisce la mappatura delle colonne da modificare e se necessario anche la
        //        // stringa da inserire nel comando Sql CONCAT

        public static async Task<bool> NormTabImp(string tipoImportazione, string dblavoro)
        {
            // Carico la tabella con le azioni da svolgere dal Service
            var repo = new CaricaTabRepository<CaricaTabNorm>("pvpmo_origine", "normalizza");
            var dati = await repo.GetAllAsync();

            bool esitoGlobale = true;

            foreach (var n in dati)
            {
                // "tipoImportazione" identifica il tipo di file da lavorare ACS o EXL
                if (n.InpType != tipoImportazione && n.DbDest != dblavoro)
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

        public static async Task<List<MySqlBulkCopyColumnMapping>> CreaMapping(
            string select, string nomeDbAcs, string nomeTbUptd, string nomeWorkSheet, string nomeDbSql, string nomeTbSql, string filePath)
        {
            DataTable tabUptd = new();
            DataTable tabProd = new();
            int dif = 0;

            if (select == "ACS")
            {
                string qryAcs = $"SELECT * FROM [{nomeTbUptd}] WHERE 1=0;";
                string qrySql = $"SHOW COLUMNS FROM `{nomeTbSql}`;";

                string connSql = Conn.MysqlConn(nomeDbSql);
                string connAcs = Conn.AcsDbConn(nomeDbAcs, filePath);
                
                await AcsAsync.AcsQryTab(connAcs, qryAcs, tabUptd);
                await SqlAsync.SqlQryDataTable(connSql, qrySql, tabProd, 30); // CORRETTO: usa DataTable non NoQry
            }
            else if (select == "EXL")
            {
                string strConnExl = Conn.ExlFileConn(filePath);
                string connSql = Conn.MysqlConn(nomeDbSql);

                string qryExl = $"SELECT * FROM [{nomeWorkSheet}$] WHERE 1=0;";
                string qrySql = $"SHOW COLUMNS FROM `{nomeTbSql}`;";

                await ExlAsync.ExcQry(strConnExl, qryExl, tabUptd);
                await SqlAsync.SqlQryDataTable(connSql, qrySql, tabProd, 30);
            }
            else if (select == "SQL")
            {
                string connProd = Conn.MysqlConn("pmo");
                string qryProd = $"SHOW COLUMNS FROM `{nomeTbSql}`;";
                await SqlAsync.SqlQryDataTable(connProd, qryProd, tabProd);
                
                string connSql = Conn.MysqlConn(nomeDbSql);
                string qryUptd = $"SHOW COLUMNS FROM `{nomeTbSql}`;";
                await SqlAsync.SqlQryDataTable(connSql, qryUptd, tabUptd);

                dif = tabProd.Rows.Count - tabUptd.Rows.Count;

                if (dif == 0)
                {
                    Mappings.Clear(); // reset mappings statici
                    
                    for (int i = 0; i < tabUptd.Rows.Count && i < tabProd.Rows.Count; i++)
                    {
                        if (tabProd.Rows[i]["Field"].ToString() != "id")
                        {
                            string? destCol = tabProd.Rows[i]["Field"]?.ToString();
                            AddMapping(i, destCol ?? $"Row{i}");
                        }
                    }
                    return Mappings;
                }
                // Colonne diverse > 1 → conferma da utente
                var confermaOk = await Shell.Current.DisplayAlert("Errore colonne",
                    $"Le colonne in Access ({tabUptd.Columns.Count}) e in SQL ({tabProd.Rows.Count}) non coincidono.\nVuoi continuare con le altre tabelle?",
                    "Si", "No");

                return Mappings;
            }

            dif = tabProd.Rows.Count - tabUptd.Columns.Count;

            if (dif == 0 || dif == 1)
            {
                Mappings.Clear(); // reset mappings statici
                int offset = dif == 1 ? 1 : 0;

                for (int i = 0; i < tabUptd.Columns.Count && (i + offset) < tabProd.Rows.Count; i++)
                {
                    string? destCol = tabProd.Rows[i + offset]["Field"]?.ToString();
                    AddMapping(i, destCol ?? $"Col{i + offset}");
                }
                return Mappings;
            }
            // Colonne diverse > 1 → conferma da utente
            var conferma = await Shell.Current.DisplayAlert("Errore colonne", 
                $"Le colonne in Access ({tabUptd.Columns.Count}) e in SQL ({tabProd.Rows.Count}) non coincidono.\nVuoi continuare con le altre tabelle?",
                "Si", "No");
            return Mappings;
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
                if (originalName == "Res_Start_Date" || originalName == "Res_Finish_Date")
                    columnType = "DATETIME";
                if (originalName == "Disapproved Timesheets" || originalName == ("Overdue Timesheets")
                    || originalName == ("Resource Depth") || originalName == ("Resource Quantity"))
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
}
