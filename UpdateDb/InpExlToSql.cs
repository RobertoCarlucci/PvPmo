namespace PvPmo.UpdateDb
{
    public partial class InpExlToSql
    {
        // Importazione file Excel per aggiornamento mensile

        public static async Task InpExl(string type, IProgress<(double percent, string tableName)>? progress = null)
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

                bool ok1 = await NomeColFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath);
                if (ok1)
                {
                    current++;
                    progress?.Report((current / (double)total, $"Read: {label}"));                    
                }

                bool ok2 = await DatiFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath, progress);
                if (ok2)
                {
                    current++;
                    progress?.Report((current / (double)total, $"Write: {label}"));                    
                }

                if (!ok1 || !ok2)
                {
                    await Shell.Current.DisplayAlert("Errore !", $"Errore su tabella: {n.TabellaSql}", "OK");
                }
            }

            if (!await NormTab.NormTabImp("EXL", "pvpmo_origine"))
            {
                await Shell.Current.DisplayAlert("Errore !", "Normalizzazione fallita.", "OK");
                return;
            }

            if (!await TestDateImpExl.FinalizzaUptd())
            {
                await Shell.Current.DisplayAlert("Errore !", "Controlli sulle tabelle importate falliti.", "OK");
                return;
            }

            await Shell.Current.DisplayAlert("Aggiornamento DB", "Aggiornamento mensile completato!", "OK");
        }

        // Crea tabella SQL da file Excel (solo struttura)

        public static async Task<bool> NomeColFileExltoTabSql(
            string nomeFoglioExl,
            string nomeDbSql,
            string nomeTbSql,
            string exlPath,
            IProgress<(double, string)>? progress = null)
        {
            DataTable schema = new();
            DataTable _tabellaExl = new();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            string QryExl = $"SELECT * FROM [{nomeFoglioExl}$] WHERE 1=0;";

            bool ok = await ExlAsync.ExcQry(StrConnExl, QryExl, _tabellaExl, progress, nomeTbSql);
            if (!ok) return false;

            string QrySql = InpExlToSql.InpExl(nomeTbSql, _tabellaExl);
            QrySql = "CREATE OR REPLACE TABLE " + QrySql;

            return await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30);
        }

        //public static async Task<bool> NomeColFileExltoTabSql(string workSheet, string nomeDbSql, string nomeTbSql, string exlPath)
        //{
        //    DataTable schema = new();

        //    string strConnExl = Conn.ExlFileConn(exlPath);
        //    string strConnSql = Conn.MysqlConn(nomeDbSql);

        //    string qrySchema = $"SELECT * FROM [{workSheet}$] WHERE 1=0;";
        //    bool schemaOk = await ExlAsync.ExcQry(strConnExl, qrySchema, schema);
        //    if (!schemaOk || schema.Columns.Count == 0) return false;

        //    string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, schema);
        //    bool ddlOk = await SqlAsync.SqlNoQry(strConnSql, ddl, 30);
        //    if (!ddlOk) return false;

        //    bool mappingOk = await NormTab.CreaMappingExlSql(strConnExl, strConnSql, workSheet, nomeDbSql, nomeTbSql);
        //    if (!mappingOk) return false;

        //    return mappingOk;
        //}

        // Copia i dati dal file Excel alla tabella SQL

        public static async Task<bool> DatiFileExltoTabSql(
            string nomeFoglio,
            string nomeDbSql,
            string nomeTbSql,
            string exlPath,
            IProgress<(double, string)>? progress = null)
        {
            DataTable _tabella = new DataTable();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string QryExl = $"SELECT * FROM [{nomeFoglio}$]";

            bool letturaOk = await ExlAsync.ExcQry(StrConnExl, QryExl, _tabella, progress, nomeTbSql); ;
            if (!letturaOk || _tabella.Rows.Count == 0)
                return false;

            string StrConnSql = Conn.MysqlConn(nomeDbSql);

            bool bulkOk = await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella, 120, progress);
            return bulkOk;
        }       
    }
}
