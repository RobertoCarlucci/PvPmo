using DocumentFormat.OpenXml.Vml.Spreadsheet;
using PvPmo.View;

namespace PvPmo.UpdateDb
{
    public partial class TestDateImpExl
    {
        // Test delle tabelle di Uptd prima di procedere all'aggiornamento
        // del Db di produzione.

        public static async Task<bool> FinalizzaUptd(IProgress<string>? progress)
        {
            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(Conn.MysqlConn("pvpmo_origine"));

            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
            var dati = await repo.GetAllAsync();
            string passLabel = string.Empty;

            bool tuttoOk = true;

            // Se non ci sono tabelle da testare esco.
            // Eseguo i test sui file exl di update importati in pmo_origine.

            foreach (var n in dati)
            {
                var db = string.IsNullOrWhiteSpace(n.DbTabConfronto) ? "default" : n.DbTabConfronto;
                var key = $"{n.TabConfronto}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{n.TabConfronto} ({db})";
                //passLabel = label.ToString();
                switch(n.Azione)
                {
                    case "TST":
                        if (string.IsNullOrWhiteSpace(n.DbTabConfronto) || string.IsNullOrWhiteSpace(n.TabConfronto) ||
                            string.IsNullOrWhiteSpace(n.ColConfronto) || string.IsNullOrWhiteSpace(n.DbTabTest) ||
                            string.IsNullOrWhiteSpace(n.TabTestare) || string.IsNullOrWhiteSpace(n.ColDaTestare))
                        {
                            await Shell.Current.DisplayAlert("Errore", "Dati di test non completi.", "OK");
                            return false;
                        }
                        tuttoOk = await EseguiTST(n.DbTabConfronto, n.TabConfronto,  n.DbTabTest, 
                            n.TabTestare, label, n.ColConfronto, n.ColDaTestare, progress);
                        if (!tuttoOk) return false;
                        break;
                    case "NUM":
                        if (string.IsNullOrWhiteSpace(n.DbTabConfronto) || string.IsNullOrWhiteSpace(n.TabConfronto) ||
                            string.IsNullOrWhiteSpace(n.DbTabTest) || string.IsNullOrWhiteSpace(n.TabTestare))
                        {
                            await Shell.Current.DisplayAlert("Errore", "Dati di test non completi.", "OK");
                            return false;
                        }
                        tuttoOk = await EseguiNUM(n.DbTabConfronto, n.TabConfronto, n.DbTabTest, n.TabTestare, label, progress);
                        if (!tuttoOk) return false;
                        break;
                    case "UPTD":
                        continue; // Non eseguo UPTD qui, ma alla fine di tutti i test.
                    default:
                        await Shell.Current.DisplayAlert("Azione sconosciuta", $"Azione {n.Azione} non riconosciuta.", "OK");
                        return !tuttoOk;
                }                
            // Eseguo update delle tabelle tra uptd e produzione.            
            }
            dati = await repo.GetAllAsync();
            if (tuttoOk == true)
            {

                foreach (var u in dati.Where(u => u.Azione == "UPTD"))                    
                {
                    var db = string.IsNullOrWhiteSpace(u.DbTabConfronto) ? "default" : u.DbTabConfronto;
                    var key = $"{u.TabConfronto}|{db}";
                    var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{u.TabConfronto} ({db})";

                    //if (string.IsNullOrWhiteSpace(u.DbTabConfronto) || string.IsNullOrWhiteSpace(u.TabConfronto) ||
                    //    string.IsNullOrWhiteSpace(u.DbTabTest) || string.IsNullOrWhiteSpace(u.TabTestare) ||
                    //    string.IsNullOrWhiteSpace(u.ColConfronto) || string.IsNullOrWhiteSpace(u.ColDaTestare))
                    //{
                    //    await Shell.Current.DisplayAlert("Errore", "Dati di update non completi.", "OK");
                    //    return false;
                    //}
                    tuttoOk = await EseguiUPTD(u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabTestare,
                            label, u.ColConfronto, u.ColDaTestare, progress);
                }
            }
            return true;          
        }        
        public static async Task<bool> EseguiTST(string dbtabConfronto, string tabConfronto, string dbTabTest, string tabTestare, 
            string label, string colConfronto = null, string colDaTestare = null, IProgress<string>? progress = null)
        {
            // Test Valore Data e Record Data presenti in Uptd
            // Confrontandolo con la tabella 01_tabella_data del Db di produzione.

            DataTable dataProd = new();
            DataTable dataUptd = new();

            string connProd = Conn.MysqlConn(dbtabConfronto);
            string connUptd = Conn.MysqlConn(dbTabTest);

            bool tuttoOk = true;

            string qProd = $"SELECT `{colConfronto}` FROM `{tabConfronto}` ORDER BY `{colConfronto}` ASC;";
            string qUptd = $"SELECT DISTINCT `{colDaTestare}` FROM `{tabTestare}` ORDER BY `{colDaTestare}` ASC;";

            progress?.Report($"Struct: {label}");

            await SqlAsync.SqlQryDataTable(connProd, qProd, dataProd, 60);
            await SqlAsync.SqlQryDataTable(connUptd, qUptd, dataUptd, 60);

            // Rimuovo righe nulle da produzione.
            // Se non lo faccio mi da errore di confronto.

            dataProd = dataProd.AsEnumerable()
                .Where(r => !string.IsNullOrWhiteSpace(r[colConfronto]?.ToString()))
                .CopyToDataTable();

            progress?.Report($"Test: {label}");

            int countProd = dataProd.Rows.Count;
            int countUptd = dataUptd.Rows.Count;

            if (countUptd != countProd)
            {
                await Shell.Current.DisplayAlert("Test righe data", "Numero righe diverso tra produzione e uptd.", "OK");
                return false;
            }

            // Viene testato il valore delle date se corrispondenti.
            // Confronto i valori delle date tra produzione e uptd.

            var valoriProd = dataProd.AsEnumerable().Select(r => r[colConfronto]?.ToString()).ToHashSet();
            var valoriUptd = dataUptd.AsEnumerable().Select(r => r[colDaTestare]?.ToString()).ToHashSet();

            if (!valoriProd.SetEquals(valoriUptd))
            {
                bool correzione = await Shell.Current.DisplayAlert(
                    "Date non corrispondenti",
                    "I valori delle date tra tabella di produzione e aggiornamento non corrispondono. Vuoi correggerli manualmente?",
                    "Sì", "No");

                if (!correzione)
                {
                    tuttoOk = false;
                    //await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    var vm = ServiceHelper.GetService<ModDataViewModel>();
                    await Shell.Current.GoToAsync(nameof(ModData));
                    //await Shell.Current.GoToAsync("//MainPage");
                    return false; // fermiamo il test corrente, verrà rieseguito dopo la modifica
                }
            }
            return tuttoOk;
        }
        public static async Task<bool> EseguiNUM(string dbtabConfronto, string tabConfronto, string dbTabTest, string tabTestare, 
            string label, IProgress<string>? progress = null)
        {
            DataTable dataProd = new();
            DataTable dataUptd = new();

            string connProd = Conn.MysqlConn(dbtabConfronto);
            string connUptd = Conn.MysqlConn(dbTabTest);

            bool tuttoOk = true;

            string showProd = $"SHOW COLUMNS FROM `{tabConfronto}`;";
            string showUptd = $"SHOW COLUMNS FROM `{tabTestare}`;";

            progress?.Report($"Test: {label}");

            await SqlAsync.SqlQryDataTable(connProd, showProd, dataProd, 30);
            await SqlAsync.SqlQryDataTable(connUptd, showUptd, dataUptd, 30);

            if (dataUptd.Rows.Count != dataProd.Rows.Count)
            {
                await Shell.Current.DisplayAlert("Test colonne",
                    $"Numero colonne diverso tra produzione e uptd tabella: ({tabConfronto}).", "OK");
                return false;
                //await Shell.Current.GoToAsync("//MainPage");
            }
            return tuttoOk;
        }

        public static async Task<bool> EseguiUPTD(string dbtabConfronto, string tabConfronto, string dbTabTest, string tabTestare, 
            string label, string? colConfronto = null, string? colDaTestare = null,  IProgress<string>? progress = null)
        {
            DataTable dataProd = new();
            DataTable dataUptd = new();

            string connProd = Conn.MysqlConn(dbtabConfronto + ";Convert Zero Datetime=True;AllowLoadLocalInfile=true;");
            string connUptd = Conn.MysqlConn(dbTabTest + ";Convert Zero Datetime=True");

            bool tuttoOk = true;
            if (tabConfronto is "pv_total" or "global_timesheet_extract")
            {
                List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

                // 1. Elimina righe dalla produzione dove il DateId coincide
                string deleteSql = $@"
                    DELETE `{tabConfronto}`.* FROM `{tabConfronto}` INNER JOIN `{tabTestare}` ON
                    `{tabConfronto}`.`{colConfronto}` = `{tabTestare}`.`{colDaTestare}`;";
                progress?.Report($"Struct: {label}");
                tuttoOk = await SqlAsync.SqlNoQry(connProd, deleteSql, 60);
                if (!tuttoOk) return false;

                // 2. Crea Mapping tra le colonne della tabella di produzione e quella di update.
                progress?.Report($"Mapping: {label}");
                Mappings = await DbUtlil.MyMapping("SQL", dbtabConfronto, tabConfronto, dbTabTest, tabTestare, null, null);

                // 3. Leggi tabella da importare
                string loadSql = $@"SELECT * FROM `{tabTestare}`;";
                DataTable dt = new DataTable();
                progress?.Report($"Load: {label}");
                await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);
                if (dt.Rows.Count == 0) return false;

                // 4. Inserisci le nuove righe dalla tabella di update
                progress?.Report($"Write: {label}");
                tuttoOk = await SqlAsync.SqlBulkCopy(connProd, tabConfronto, dt, Mappings, 180);
                if (!tuttoOk) return false;

            }
            else if (tabConfronto is "all_project_mapped_power_bi_column_set" or "timesheet_information_by_month"
                    && !string.IsNullOrEmpty(dbtabConfronto))
            {
                List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

                // 1. Crea Mapping tra le colonne della tabella di produzione e quella di update.
                progress?.Report($"Mapping: {label}");
                Mappings = await DbUtlil.MyMapping("SQL", dbtabConfronto, tabConfronto, dbTabTest, tabTestare, null, null);
                if (Mappings == null) return false;

                // 2. Leggi tabella da importare
                string loadSql = $@"SELECT * FROM `{tabConfronto}`;";
                DataTable dt = new DataTable();
                progress?.Report($"Load: {label}");
                await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);
                if (dt.Rows.Count == 0) return false;

                // 3. Cancella i dati esistenti nella tabella di produzione.
                string truncateSql = $"TRUNCATE TABLE `{tabConfronto}`;";
                progress?.Report($"Struct: {label}");
                tuttoOk = await SqlAsync.SqlNoQry(connProd, truncateSql, 60);
                if (!tuttoOk) return false;

                // 4. Inserisci le nuove righe dalla tabella di update
                progress?.Report($"Write: {label}");
                tuttoOk = await SqlAsync.SqlBulkCopy(connProd, tabConfronto, dt, Mappings, 180);
                if (!tuttoOk) return false;

                if (tabConfronto is "all_project_mapped_power_bi_column_set" && tuttoOk is true)
                {
                    string cancSql = $"CREATE TABLE;";
                    progress?.Report($"Struct: {label}");
                    tuttoOk = await SqlAsync.SqlNoQry(connProd, cancSql, 60);
                    if (!tuttoOk) return false;
                }
            }
            return tuttoOk;
        }       

    //    // Eseguo i test sui file exl di update importati in pmo_origine
    //    public static async Task<bool> TestFileExlImp(string azione, string tabProd, 
    //        string colProd, string tabUptd, string colUptd, string dbProd, string dbUptd, string label, IProgress<string>? progress = null)
    //    {
    //        bool tuttoOk = true;

    //        // Produzione corrisponde alla tabella di Uptd.
    //        // La tabella di produzione è quella di confronto.
    //        // La tabella di uptd è quella da testare.

    //        DataTable dataProd = new();
    //        DataTable dataUptd = new();

    //        string connProd = Conn.MysqlConn(dbProd);
    //        string connUptd = Conn.MysqlConn(dbUptd);
                        
    //        switch (azione)
    //        {
    //            //Test Valore Data e Record Data presenti in Uptd
    //            // Confrontandolo con la tabella 01_tabella_data del Db di produzione.

    //            case "TST":
    //                string qProd = $"SELECT `{colProd}` FROM `{tabProd}` ORDER BY `{colProd}` ASC;";
    //                string qUptd = $"SELECT DISTINCT `{colUptd}` FROM `{tabUptd}` ORDER BY `{colUptd}` ASC;";

    //                progress?.Report($"Struct: {label}");

    //                await SqlAsync.SqlQryDataTable(connProd, qProd, dataProd, 60);
    //                await SqlAsync.SqlQryDataTable(connUptd, qUptd, dataUptd, 60);

    //                // Rimuovo righe nulle da produzione.
    //                // Se non lo faccio mi da errore di confronto.

    //                dataProd = dataProd.AsEnumerable()
    //                    .Where(r => !string.IsNullOrWhiteSpace(r[colProd]?.ToString()))
    //                    .CopyToDataTable();

    //                progress?.Report($"Test: {label}");

    //                int countProd = dataProd.Rows.Count;
    //                int countUptd = dataUptd.Rows.Count;

    //                if (countUptd != countProd)
    //                {
    //                    await Shell.Current.DisplayAlert("Test righe data", "Numero righe diverso tra produzione e uptd.", "OK");
    //                    return false;                                           
    //                }

    //                // Viene testato il valore delle date se corrispondenti.
    //                // Confronto i valori delle date tra produzione e uptd.

    //                var valoriProd = dataProd.AsEnumerable().Select(r => r[colProd]?.ToString()).ToHashSet();
    //                var valoriUptd = dataUptd.AsEnumerable().Select(r => r[colUptd]?.ToString()).ToHashSet();

    //                if (!valoriProd.SetEquals(valoriUptd))
    //                {
    //                    bool correzione = await Shell.Current.DisplayAlert(
    //                        "Date non corrispondenti",
    //                        "I valori delle date tra produzione e aggiornamento non corrispondono.\nVuoi correggerli manualmente?",
    //                        "Sì", "No");

    //                    if (!correzione)
    //                    {
    //                        //tuttoOk = false;
    //                        await Shell.Current.GoToAsync("//MainPage");                            
    //                    }
    //                    else
    //                    {
    //                        var vm = ServiceHelper.GetService<ModDataViewModel>();                                
    //                        await Shell.Current.GoToAsync(nameof(ModData));
    //                        //await Shell.Current.GoToAsync("//MainPage");
    //                        return false; // fermiamo il test corrente, verrà rieseguito dopo la modifica
    //                    }
    //                }
    //                break;

    //            case "NUM":

    //                // Viene testato il numero delle colonne presenti in Uptd rispetto alle
    //                // tabelle presenti nel Db di produzione.
                        
    //                string showProd = $"SHOW COLUMNS FROM `{tabProd}`;";
    //                string showUptd = $"SHOW COLUMNS FROM `{tabUptd}`;";

    //                progress?.Report($"Test: {label}");

    //                await SqlAsync.SqlQryDataTable(connProd, showProd, dataProd, 30);
    //                await SqlAsync.SqlQryDataTable(connUptd, showUptd, dataUptd, 30);

    //                if (dataUptd.Rows.Count != dataProd.Rows.Count)
    //                {
    //                    await Shell.Current.DisplayAlert("Test colonne",
    //                        $"Numero colonne diverso tra produzione e uptd tabella: ({tabProd}).", "OK");
    //                    return false;
    //                    //await Shell.Current.GoToAsync("//MainPage");
    //                }
    //                break;                        
    //        }
    //        return tuttoOk;            
    //    }        
    }
}
