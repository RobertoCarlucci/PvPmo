namespace PvPmo.Import
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
                string _connExl = string.Empty;
                string _connUptd = string.Empty;

                _connExl = (!string.IsNullOrEmpty(fullPath)) ? _connExl = await Conn.ExlFileConn(fullPath) : _connExl;
                if (string.IsNullOrEmpty(_connExl))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al File.", "OK");
                    return false;
                }

                _connUptd = (!string.IsNullOrEmpty(dbSql)) ? _connUptd = await Conn.MysqlConn(dbSql + "; Convert Zero Datetime=True") : _connUptd;
                if (string.IsNullOrEmpty(_connUptd))
                {
                    await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                    return false;
                }

                DataTable schema = new();
                string qrySchema = $"SELECT * FROM [{workSheet}$] WHERE 1=0;";
                bool schemaOk = await ExlAsync.ExcQry(_connExl, qrySchema, schema);
                if (!schemaOk) return false;

                string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(tabSql, schema);
                bool ddlOk = await SqlAsync.SqlNoQryString(_connUptd, ddl, 30);
                if (!ddlOk) return false;

                List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
                Mappings = await DbUtlil.MyMapping("EXL", dbSql, tabSql, null, null, workSheet, fullPath);
                if (Mappings == null) return false;

                string qryData = $"SELECT * FROM [{workSheet}$];";
                DataTable dati = new();
                bool dataOk = await ExlAsync.ExcQry(_connExl, qryData, dati);
                if (!dataOk || dati.Rows.Count == 0) return false;

                bool insertOk = await SqlAsync.SqlBulkCopy(_connUptd, tabSql, dati, Mappings, 30);
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
