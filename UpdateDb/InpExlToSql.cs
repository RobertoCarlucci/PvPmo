namespace PvPmo.UpdateDb
{
    public partial class InpExlToSql()
    {
        // Importazione file Excel per aggiornamento mensile

        public static async Task InpExl(string type, IProgress<string>? progress = null)
        {
            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath)) return;

            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(Conn.MysqlConn("pvpmo_origine"));
            
            var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
            var dati = await repo.GetAllAsync();

            var validi = dati.Where(n =>
                n.InpType == type &&
                !string.IsNullOrWhiteSpace(n.Tabella) &&
                !string.IsNullOrWhiteSpace(n.DbDest) &&
                !string.IsNullOrWhiteSpace(n.TabellaSql) &&
                !string.IsNullOrWhiteSpace(n.WorkSheet)).ToList();

            int total = validi.Count * 2;
            int current = 0;

            foreach (var n in validi)
            {
                var db = string.IsNullOrWhiteSpace(n.DbDest) ? "default" : n.DbDest;
                var key = $"{n.TabellaSql}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{n.TabellaSql} ({db})";                        

                string filePath = Path.Combine(exlPath, n.Tabella!);                

                bool inprtOk = await impFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath, label, progress);
                if (inprtOk) { current++; }              

                if (!inprtOk)
                {
                    await Shell.Current.DisplayAlert("Errore su tabella!", $"Errore su: {n.TabellaSql} " +
                        $"non è possibile proseguire.", "OK");
                    await Shell.Current.GoToAsync("//MainPage");
                }
            }

            if (!await NormTab.NormTabImp("EXL", "pvpmo_origine"))
            {
                await Shell.Current.DisplayAlert("Errore !", "Normalizzazione fallita.", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }

            if (!await TestDateImpExl.FinalizzaUptd(progress))
            {
                await Shell.Current.DisplayAlert("Errore !", "Controlli sulle tabelle importate falliti.", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }

            if (!await FinalUpdtProdDb.UptdKey(progress))
            {
                await Shell.Current.DisplayAlert("Errore !", "Controlli sulle tabelle importate falliti.", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }

            await Shell.Current.DisplayAlert("Aggiornamento DB", "Aggiornamento mensile completato!", "OK");
        }              

        public static async Task<bool> impFileExltoTabSql(
            string workSheet, string nomeDbSql, string nomeTbSql, string exlPath, string label, IProgress<string>? progress = null)
        {
            // Crea tabella SQL da file Excel (solo struttura)  

            DataTable schema = new();
            bool mappingOk = true;

            string strConnExl = Conn.ExlFileConn(exlPath);
            string strConnSql = Conn.MysqlConn(nomeDbSql);

            string qrySchema = $"SELECT * FROM [{workSheet}$] WHERE 1=0;";
            progress?.Report($"Struct: {label}");
            bool schemaOk = await ExlAsync.ExcQry(strConnExl, qrySchema, schema);
            if (!schemaOk || schema.Columns.Count == 0) return false;

            string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, schema);
            progress?.Report($"Struct: {label}");
            bool ddlOk = await SqlAsync.SqlNoQry(strConnSql, ddl, 60);
            if (!ddlOk) return false;

            List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
            progress?.Report($"Mapping: {label}");
            Mappings = await NormTab.CreaMapping("EXL", "NotUsed", "NotUsed", workSheet, nomeDbSql, nomeTbSql, exlPath);
            if (Mappings == null) return false;

            // Copia i dati dal file Excel alla tabella SQL

            DataTable _tabella = new DataTable();

            string QryExl = $"SELECT * FROM [{workSheet}$]";
            progress?.Report($"Load: {label}");
            bool letturaOk = await ExlAsync.ExcQry(strConnExl, QryExl, _tabella);
            if (!letturaOk || _tabella.Rows.Count == 0) return false;

            progress?.Report($"Write: {label}");
            bool bulkOk = await SqlAsync.SqlBulkCopy(strConnSql, nomeTbSql, _tabella, Mappings, 60);
            return bulkOk;            
        }
    }
}