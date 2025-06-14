using PvPmo.View;

namespace PvPmo.Import
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

            bool tuttoOk = true;

            // Se non ci sono tabelle da testare esco.
            // Eseguo i test sui file exl di update importati in pmo_origine.

            foreach (var n in dati)
            {
                var db = string.IsNullOrWhiteSpace(n.DbTabConfronto) ? "default" : n.DbTabConfronto;
                var key = $"{n.TabConfronto}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{n.TabConfronto} ({db})";
               
                switch (n.Azione)
                {
                    case "TST":
                        if (string.IsNullOrWhiteSpace(n.DbTabConfronto) || string.IsNullOrWhiteSpace(n.TabConfronto) ||
                            string.IsNullOrWhiteSpace(n.ColConfronto) || string.IsNullOrWhiteSpace(n.DbTabTest) ||
                            string.IsNullOrWhiteSpace(n.TabTestare) || string.IsNullOrWhiteSpace(n.ColDaTestare))
                        {
                            await Shell.Current.DisplayAlert("Errore", "Dati di test non completi.", "OK");
                            return false;
                        }
                        tuttoOk = await EseguiTST(n.DbTabConfronto, n.TabConfronto, n.DbTabTest,
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
            }
            //dati = await repo.GetAllAsync();
            //if (tuttoOk == true)
            //{
            //    bool uptdOk = true;
            //    foreach (var u in dati.Where(u => u.Azione == "UPTD"))
            //    {
            //        if (uptdOk == true)
            //        {
            //            var db = string.IsNullOrWhiteSpace(u.DbTabConfronto) ? "default" : u.DbTabConfronto;
            //            var key = $"{u.TabConfronto}|{db}";
            //            var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{u.TabConfronto} ({db})";

            //            uptdOk = await EsgUptdTabProd(u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabTestare,
            //                    label, u.ColConfronto, u.ColDaTestare, progress);
            //        }
            //        tuttoOk = false; // Se almeno un test fallisce, non procedo con l'aggiornamento.
            //    }
            //}
            tuttoOk = await UptdProd.EsgUptdTabProd("pvpmo_origine", "pvpmo_uptd", progress);
            return tuttoOk;
        }

        public static async Task<bool> EseguiTST(string dbtabConfronto, string tabConfronto, string dbTabTest, string tabTestare,
            string label, string colConfronto, string colDaTestare, IProgress<string>? progress = null)
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
                    "I valori delle date tra tabella di produzione e aggiornamento non corrispondono. \nVuoi correggerli manualmente?",
                    "Sì", "No");

                if (!correzione)
                {
                    return false;                     
                }
                else
                {
                    var vm = ServiceHelper.GetService<ModDataViewModel>();
                    await Shell.Current.GoToAsync(nameof(ModData));
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

            await SqlAsync.SqlQryDataTable(connProd, showProd, dataProd, 60);
            await SqlAsync.SqlQryDataTable(connUptd, showUptd, dataUptd, 60);

            if (dataUptd.Rows.Count != dataProd.Rows.Count)
            {
                await Shell.Current.DisplayAlert("Test colonne",
                    $"Numero colonne diverso tra produzione e uptd tabella: ({tabConfronto}).", "OK");
                return false;
            }
            return tuttoOk;
        }
        //public static async Task<bool> EsgUptdTabProd(string dbtabConfronto, string tabConfronto, string dbTabTest, string tabTestare,
        //    string label, string colConfronto, string colDaTestare, IProgress<string>? progress = null)
        //{
        //    DataTable dataProd = new();
        //    DataTable dataUptd = new();

        //    string connProd = Conn.MysqlConn(dbtabConfronto + ";AllowLoadLocalInfile=true");
        //    string connUptd = Conn.MysqlConn(dbTabTest + ";Convert Zero Datetime=True");

        //    bool tuttoOk = true;

        //    if (tabConfronto is "pv_total" or "global_timesheet_extract")
        //    {
        //        List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

        //        // 1. Crea Mapping tra le colonne della tabella di produzione e quella di update.
        //        progress?.Report($"Mapping: {label}");
        //        Mappings = await DbUtlil.MyMapping("SQL", dbtabConfronto, tabConfronto, dbTabTest, tabConfronto, null, null);

        //        // 2. Leggi tabella da importare
        //        string loadSql = $@"SELECT * FROM `{tabConfronto}`;";
        //        DataTable dt = new DataTable();
        //            progress?.Report($"Load: {label}");
        //            await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);
        //            if (dt.Rows.Count == 0) return false;

        //        using (MySqlConnection myConnection = new MySqlConnection(connProd))
        //        {
        //            await myConnection.OpenAsync();
        //            // Start a local transaction
        //            MySqlTransaction myTrans = myConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        //            MySqlCommand myCommand = myConnection.CreateCommand();
        //            myCommand.Transaction = myTrans;
        //            try
        //            {
        //                myCommand.CommandText = $@"DELETE `{tabConfronto}`.* FROM `{tabConfronto}` INNER JOIN `{tabTestare}` ON 
        //                    `{tabConfronto}`.`{colConfronto}` = `{tabTestare}`.`{colDaTestare}`;";
        //                progress?.Report($"Clean: {label}");
        //                await myCommand.ExecuteNonQueryAsync();                        
        //                progress?.Report($"Write: {label}");
        //                var bulk = new MySqlBulkCopy(myConnection, myTrans)
        //                {
        //                    DestinationTableName = tabConfronto,                            
        //                    BulkCopyTimeout = 240
        //                };
        //                if (Mappings != null)
        //                {
        //                    Mappings.ForEach(_mapping => { bulk.ColumnMappings.Add(_mapping); });
        //                    Mappings.Clear(); // This line is safe now because we check for null above
        //                }
        //                await bulk.WriteToServerAsync(dt);
        //                await myTrans.CommitAsync();
        //            }
        //            catch (MySqlException ex)
        //            {
        //                await myTrans.RollbackAsync();
        //                await DbErrorHandler.ShowErrorAsync(ex, @$"Esecuzione EsgUptdTabProd '{tabConfronto}' Transaction SQL");
        //                return false;
        //            }
        //            finally
        //            {
        //                myCommand.Dispose();
        //                myTrans.Dispose();
        //                if (myConnection != null && myConnection.State == ConnectionState.Open) // Fixed condition
        //                {
        //                    await myConnection.CloseAsync();
        //                }
        //            }
        //            return true;
        //        }
        //    }
        //    else if (tabConfronto is "all_project_mapped_power_bi_column_set" or "timesheet_information_by_month")
        //    {
        //        List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

        //        // 1. Crea Mapping tra le colonne della tabella di produzione e quella di update.
        //        progress?.Report($"Mapping: {label}");
        //        Mappings = await DbUtlil.MyMapping("SQL", dbtabConfronto, tabConfronto, dbTabTest, tabConfronto, null, null);
        //        if (Mappings == null) return false;

        //        // 2. Leggi tabella da importare
        //        string loadSql = $@"SELECT * FROM `{tabConfronto}`;";
        //        DataTable dt = new DataTable();
        //        progress?.Report($"Load: {label}");
        //        await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);
        //        if (dt.Rows.Count == 0) return false;
        //        using (MySqlConnection myConnection = new MySqlConnection(connProd))
        //        {
        //            await myConnection.OpenAsync();
        //            // Start a local transaction
        //            MySqlTransaction myTrans = myConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        //            MySqlCommand myCommand = myConnection.CreateCommand();
        //            myCommand.Transaction = myTrans;
        //            try
        //            {
        //                myCommand.CommandText = $@"TRUNCATE TABLE `{tabConfronto}`;";
        //                progress?.Report($"Clean: {label}");
        //                await myCommand.ExecuteNonQueryAsync();                        
        //                progress?.Report($"Write: {label}");
        //                var bulk = new MySqlBulkCopy(myConnection, myTrans)
        //                {
        //                    DestinationTableName = tabConfronto,                            
        //                    BulkCopyTimeout = 240
        //                };
        //                if (Mappings != null)
        //                {
        //                    Mappings.ForEach(_mapping => { bulk.ColumnMappings.Add(_mapping); });
        //                    Mappings.Clear(); // This line is safe now because we check for null above
        //                }
                        
        //                await bulk.WriteToServerAsync(dt);
        //                await myTrans.CommitAsync();
        //            }
        //            catch (MySqlException ex)
        //            {
        //                await myTrans.RollbackAsync();
        //                await DbErrorHandler.ShowErrorAsync(ex, @$"Esecuzione EsgUptdTabProd '{tabConfronto}' Transaction SQL");
        //                return false;
        //            }
        //            finally
        //            {
        //                myCommand.Dispose();
        //                myTrans.Dispose();
        //                if (myConnection != null && myConnection.State == ConnectionState.Open) // Fixed condition
        //                {
        //                    await myConnection.CloseAsync();
        //                }
        //            }
        //            if (tabConfronto is "all_project_mapped_power_bi_column_set" && tuttoOk is true)
        //            {
        //                string cancSql = $"DELETE `{tabConfronto}`.* FROM `{tabConfronto}` WHERE `SequenceID` IS NULL;";
        //                progress?.Report($"Clean: {label}");
        //                tuttoOk = await SqlAsync.SqlNoQry(connProd, cancSql, 60);
        //                if (!tuttoOk) return false;
        //            }                    
        //        }                
        //    }            
                       
        //}

    }

}
