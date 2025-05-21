namespace PvPmo.ImportDb
{
    public partial class InpExlSetup
    {
        public static async  Task NewSetup()
        {
            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath))
            {
                await Shell.Current.DisplayAlert("Errore selezione cartella !", "Non è stata effettuata alcuna selezione.", "OK");
                return;
            }

            bool allOk = true;

            allOk &= await ImportSetup(exlPath, "NormalizzaInp", "Foglio 1", "pvpmo_origine", "normalizza");
            allOk &= await ImportSetup(exlPath, "Origine", "Foglio1", "pvpmo_origine", "origine");
            allOk &= await ImportSetup(exlPath, "finalizzatest", "Foglio1", "pvpmo_origine", "finalizza");
            allOk &= await ImportSetup(exlPath, "progress_descrizione", "Foglio1", "pvpmo_origine", "progress_descrizione");

            if (!allOk)
            {
                await Shell.Current.DisplayAlert("Errore importazione !", "Uno o più file non sono stati importati correttamente.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Importazione completata !", "Tutti i file sono stati importati correttamente.", "OK");
            }
        }

        public static async Task<bool> ImportSetup(string exlPath, string nomeFile, string workSheet, string dbSql, string tabSql)
        {
            try
            {
                string fullPath = $"{exlPath}\\{nomeFile}";
                //string fullPath = Path.Combine(exlPath, nomeFile);
                string connExl = Conn.ExlFileConn(fullPath);
                string connSql = Conn.MysqlConn(dbSql);

                DataTable schema = new();
                string qrySchema = $"SELECT * FROM [{workSheet}$] WHERE 1=0;";
                bool schemaOk = await ExlAsync.ExcQry(connExl, qrySchema, schema);
                if (!schemaOk) return false;

                string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(tabSql, schema);
                bool ddlOk = await SqlAsync.SqlNoQry(connSql, ddl, 30);
                if (!ddlOk) return false;

                bool mappingOk = await NormTab.CreaMappingExlSql(connExl, connSql, workSheet, dbSql, tabSql);
                if (!mappingOk) return false;

                string qryData = $"SELECT * FROM [{workSheet}$];";
                DataTable dati = new();
                bool dataOk = await ExlAsync.ExcQry(connExl, qryData, dati);
                if (!dataOk || dati.Rows.Count == 0) return false;

                bool insertOk = await SqlAsync.SqlBulkCopy(connSql, tabSql, dati, 30);
                return insertOk;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore Import", $"Errore nella tabella: {tabSql}\n{ex.Message}", "OK");
                return false;
            }
        }
    }
}
