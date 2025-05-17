namespace PvPmo.UpdateDb
{
    public partial class InpExlToSql
    {
        // Importazione file Excel per aggiornamento mensile

        public static async Task InpExl(string type, IProgress<(double percent, string tableName)>? progress = null)
        {
            string? exlPath = await SelCart.PickFolder();
            if (string.IsNullOrWhiteSpace(exlPath)) return;

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
                string filePath = Path.Combine(exlPath, n.Tabella!);

                bool ok1 = await NomeColFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath);
                if (ok1)
                {
                    current++;
                    progress?.Report((current / (double)total, $"Read: {n.TabellaSql}"));
                }

                bool ok2 = await DatiFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath, progress);
                if (ok2)
                {
                    current++;
                    progress?.Report((current / (double)total, $"Write: {n.TabellaSql}"));
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


        //public static async Task InpExl(string type, IProgress<(double percent, string tableName)>? progress = null)
        //{
        //    string? exlPath = await SelCart.PickFolder();
        //    if (string.IsNullOrWhiteSpace(exlPath)) return;

        //    var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
        //    var dati = await repo.GetAllAsync();

        //    var validi = dati.Where(n =>
        //        n.InpType == type &&
        //        !string.IsNullOrWhiteSpace(n.Tabella) &&
        //        !string.IsNullOrWhiteSpace(n.DbDest) &&
        //        !string.IsNullOrWhiteSpace(n.TabellaSql) &&
        //        !string.IsNullOrWhiteSpace(n.WorkSheet)).ToList();

        //    int total = validi.Count * 2;
        //    int current = 0;

        //    foreach (var n in validi)
        //    {
        //        string filePath = Path.Combine(exlPath, n.Tabella!);

        //        bool ok1 = await NomeColFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath);
        //        if (ok1)
        //        {
        //            current++;
        //            progress?.Report((current / (double)total, $"Struttura: {n.TabellaSql}"));
        //        }

        //        bool ok2 = await DatiFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath, progress);
        //        if (ok2)
        //        {
        //            current++;
        //            progress?.Report((current / (double)total, $"Dati: {n.TabellaSql}"));
        //        }

        //        if (!ok1 || !ok2)
        //        {
        //            await Shell.Current.DisplayAlert("Errore !", $"Errore su tabella: {n.TabellaSql}", "OK");
        //        }
        //    }

        //    if (!await NormTab.NormTabImp("EXL", "pvpmo_origine"))
        //    {
        //        await Shell.Current.DisplayAlert("Errore !", "Normalizzazione fallita.", "OK");
        //        return;
        //    }

        //    if (!await TestDateImpExl.FinalizzaUptd())
        //    {
        //        await Shell.Current.DisplayAlert("Errore !", "Controlli sulle tabelle importate falliti.", "OK");
        //        return;
        //    }

        //    await Shell.Current.DisplayAlert("Aggiornamento DB", "Aggiornamento mensile completato!", "OK");
        //}

        //public static async Task InpExl(string type, IProgress<(double percent, string tableName)>? progress = null)
        //{
        //    string? exlPath = await SelCart.PickFolder();
        //    if (string.IsNullOrWhiteSpace(exlPath)) return;

        //    var repo = new CaricaTabRepository<CaricaTabOrigini>("pvpmo_origine", "origine");
        //    var dati = await repo.GetAllAsync();

        //    var validi = dati.Where(n =>
        //        n.InpType == type &&
        //        !string.IsNullOrWhiteSpace(n.Tabella) &&
        //        !string.IsNullOrWhiteSpace(n.DbDest) &&
        //        !string.IsNullOrWhiteSpace(n.TabellaSql) &&
        //        !string.IsNullOrWhiteSpace(n.WorkSheet)).ToList();

        //    int total = validi.Count * 2;
        //    int current = 0;

        //    foreach (var n in validi)
        //    {
        //        string filePath = Path.Combine(exlPath, n.Tabella!);

        //        bool ok1 = await NomeColFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath);
        //        current++;
        //        progress?.Report((current / (double)total, $"Struttura: {n.TabellaSql}"));

        //        bool ok2 = await DatiFileExltoTabSql(n.WorkSheet!, n.DbDest!, n.TabellaSql!, filePath);
        //        current++;
        //        progress?.Report((current / (double)total, $"Dati: {n.TabellaSql}"));

        //        if (!ok1 || !ok2)
        //        {
        //            await Shell.Current.DisplayAlert("Errore !", $"Errore su tabella: {n.TabellaSql}", "OK");
        //        }
        //    }

        //    if (!await NormTab.NormTabImp("EXL", "pvpmo_origine"))
        //    {
        //        await Shell.Current.DisplayAlert("Errore !", "Normalizzazione fallita.", "OK");
        //        return;
        //    }

        //    if (!await TestDateImpExl.FinalizzaUptd())
        //    {
        //        await Shell.Current.DisplayAlert("Errore !", "Controlli sulle tabelle importate falliti.", "OK");
        //        return;
        //    }

        //    await Shell.Current.DisplayAlert("Aggiornamento DB", "Aggiornamento mensile completato!", "OK");
        //}


        // Crea tabella SQL da file Excel (solo struttura)
        public static async Task<bool> NomeColFileExltoTabSql(string workSheet, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable schema = new();

            string strConnExl = Conn.ExlFileConn(exlPath);
            string strConnSql = Conn.MysqlConn(nomeDbSql);

            string qrySchema = $"SELECT * FROM [{workSheet}$] WHERE 1=0;";
            bool schemaOk = await ExlAsync.ExcQry(strConnExl, qrySchema, schema);
            if (!schemaOk || schema.Columns.Count == 0) return false;

            string ddl = "CREATE OR REPLACE TABLE " + NormTab.NormInp(nomeTbSql, schema);
            bool ddlOk = await SqlAsync.SqlNoQry(strConnSql, ddl, 30);
            if (!ddlOk) return false;

            bool mappingOk = await NormTab.CreaMappingExlSql(strConnExl, strConnSql, workSheet, nomeDbSql, nomeTbSql);
            if (!mappingOk) return false;

            return mappingOk;
        }

        // Copia i dati dal file Excel alla tabella SQL

        public static async Task<bool> DatiFileExltoTabSql(string nomeFoglio, string nomeDbSql, string nomeTbSql, string exlPath, IProgress<(double, string)>? progress = null)
        {
            DataTable _tabella = new DataTable();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string QryExl = $"SELECT * FROM [{nomeFoglio}$]";
            bool ok = await ExlAsync.ExcQry(StrConnExl, QryExl, _tabella);
            if (!ok || _tabella.Rows.Count == 0) return false;
            string StrConnSql = Conn.MysqlConn(nomeDbSql);

            bool insertOk = await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella, 120, progress);
            if (!insertOk) return false;
            return insertOk;
        }

        //public static async Task<bool> DatiFileExltoTabSql(string workSheet, string nomeDbSql, string nomeTbSql, string exlPath)
        //{
        //    DataTable dati = new();
        //    string strConnExl = Conn.ExlFileConn(exlPath);
        //    string qryExl = $"SELECT * FROM [{workSheet}$];"; // Carica dati reali
        //    bool ok = await ExlAsync.ExcQry(strConnExl, qryExl, dati);
        //    if (!ok || dati.Rows.Count == 0) return false;

        //    string strConnSql = Conn.MysqlConn(nomeDbSql);
        //    bool insertOk = await SqlAsync.SqlBulkCopy(strConnSql, nomeTbSql, dati, 120);
        //    if (!insertOk) return false;
        //    return insertOk;
        //}
    }
}
