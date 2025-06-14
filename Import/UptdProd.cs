namespace PvPmo.Import
{
    public partial class UptdProd
    {
        public static async Task<bool> EsgUptdTabProd(string connProd, string connUptd, IProgress<string>? progress)
        {
            string conProd = Conn.MysqlConn(connProd + ";AllowLoadLocalInfile=true");
            string conUptd = Conn.MysqlConn(connUptd + ";Convert Zero Datetime=True");

            List<MySqlBulkCopyColumnMapping> MappingsPvt = new List<MySqlBulkCopyColumnMapping>();
            List<MySqlBulkCopyColumnMapping> MappingsGtex = new List<MySqlBulkCopyColumnMapping>();
            List<MySqlBulkCopyColumnMapping> MappingsApm = new List<MySqlBulkCopyColumnMapping>();
            List<MySqlBulkCopyColumnMapping> MappingsTim = new List<MySqlBulkCopyColumnMapping>();

            DataTable dataPvt = new();
            DataTable dataGtex = new();
            DataTable dataApm = new();
            DataTable dataTim = new();

            string qryPvt = string.Empty;
            string qryGtex = string.Empty;
            string qryApm = string.Empty;
            string qryTim = string.Empty;

            string tabPvt = string.Empty;
            string tabGtex = string.Empty;
            string tabApm = string.Empty;
            string tabTim = string.Empty;

            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(Conn.MysqlConn("pvpmo_origine"));

            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
            var dati = await repo.GetAllAsync();

            bool tuttoOk = true;

            // Se non ci sono tabelle da testare esco.
            // Eseguo i test sui file exl di update importati in pmo_origine.
           
            foreach (var u in dati.Where(u => u.Azione == "UPTD"))
            {
                var db = string.IsNullOrWhiteSpace(u.DbTabConfronto) ? "default" : u.DbTabConfronto;
                var key = $"{u.TabConfronto}|{db}";
                var label = descrizioni.TryGetValue(key, out var desc) ? desc : $"{u.TabConfronto} ({db})";               

                switch (u.TabConfronto)
                {
                    case "all_project_mapped_power_bi_column_set":
                        progress?.Report($"Mapping: {label}");
                        MappingsApm = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsApm == null) return false;
                        qryApm = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(conUptd, qryApm, dataApm, 60);
                        if (dataApm.Rows.Count == 0) return false;
                        qryApm = $@"TRUNCATE TABLE `{u.TabConfronto}`;";
                        tabApm = u.TabConfronto;
                        break;
                    case "timesheet_information_by_month":
                        progress?.Report($"Mapping: {label}");
                        MappingsTim = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsTim == null) return false;
                        qryTim = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(conUptd, qryApm, dataTim, 60);
                        if (dataApm.Rows.Count == 0) return false;
                        qryTim = $@"TRUNCATE TABLE `{u.TabConfronto}`;";
                        tabTim = u.TabConfronto;
                        break;
                    case "pv_total":
                        progress?.Report($"Mapping: {label}");
                        MappingsPvt = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsPvt == null) return false;
                        qryPvt = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(conUptd, qryPvt, dataPvt, 60);
                        if (dataPvt.Rows.Count == 0) return false;
                        qryPvt = $@"DELETE `{u.TabConfronto}`.* FROM `{u.TabConfronto}` INNER JOIN `{u.TabTestare}` ON 
                            `{u.TabConfronto}`.`{u.ColConfronto}` = `{u.TabTestare}`.`{u.ColDaTestare}`;";
                        tabPvt = u.TabConfronto;
                        break;
                    case "global_timesheet_extract":
                        progress?.Report($"Mapping: {label}");
                        MappingsGtex = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsGtex == null) return false;
                        qryGtex = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(conUptd, qryGtex, dataGtex, 60);
                        if (dataGtex.Rows.Count == 0) return false;
                        qryGtex = $@"DELETE `{u.TabConfronto}`.* FROM `{u.TabConfronto}` INNER JOIN `{u.TabTestare}` ON 
                            `{u.TabConfronto}`.`{u.ColConfronto}` = `{u.TabTestare}`.`{u.ColDaTestare}`;";
                        tabGtex = u.TabConfronto;
                        break;
                    default:
                        tuttoOk = false;
                        break;
                }
            }

            using (MySqlConnection myConnection = new MySqlConnection(connProd))
            {
                await myConnection.OpenAsync();
                // Start a local transaction
                MySqlTransaction myTrans = myConnection.BeginTransaction(IsolationLevel.ReadCommitted);
                MySqlCommand myCommand = myConnection.CreateCommand();
                myCommand.Transaction = myTrans;
                try
                {
                    myCommand.CommandText = qryApm;
                    progress?.Report($"Clean: All_Project");
                    await myCommand.ExecuteNonQueryAsync();
                    myCommand.CommandText = qryTim;
                    progress?.Report($"Clean: Timesheet");
                    await myCommand.ExecuteNonQueryAsync();
                    myCommand.CommandText = qryPvt;
                    progress?.Report($"Clean: Pv_Total");
                    await myCommand.ExecuteNonQueryAsync();
                    myCommand.CommandText = qryGtex;
                    progress?.Report($"Clean: Global");
                    await myCommand.ExecuteNonQueryAsync();
                    progress?.Report($"Write: All_Projrct");
                    var bulkApm = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = tabApm, BulkCopyTimeout = 240 };
                    if (MappingsApm != null)
                    {
                        MappingsApm.ForEach(_mapping => { bulkApm.ColumnMappings.Add(_mapping); });
                        MappingsApm.Clear(); // This line is safe now because we check for null above
                    }
                    await bulkApm.WriteToServerAsync(dataApm);
                    progress?.Report($"Write: Timesheet");
                    var bulkTim = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = tabTim, BulkCopyTimeout = 240 };
                    if (MappingsTim != null)
                    {
                        MappingsTim.ForEach(_mapping => { bulkTim.ColumnMappings.Add(_mapping); });
                        MappingsTim.Clear(); // This line is safe now because we check for null above
                    }
                    await bulkTim.WriteToServerAsync(dataTim);
                    var bulkPvt = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = tabPvt, BulkCopyTimeout = 240 };
                    if (MappingsPvt != null)
                    {
                        MappingsPvt.ForEach(_mapping => { bulkPvt.ColumnMappings.Add(_mapping); });
                        MappingsPvt.Clear(); // This line is safe now because we check for null above
                    }
                    await bulkPvt.WriteToServerAsync(dataPvt);
                    var bulkGtex = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = tabGtex, BulkCopyTimeout = 240 };
                    if (MappingsGtex != null)
                    {
                        MappingsGtex.ForEach(_mapping => { bulkGtex.ColumnMappings.Add(_mapping); });
                        MappingsGtex.Clear(); // This line is safe now because we check for null above
                    }
                    await bulkGtex.WriteToServerAsync(dataGtex);
                    await myTrans.CommitAsync();                    
                }
                catch (MySqlException ex)
                {
                    await myTrans.RollbackAsync();
                    await DbErrorHandler.ShowErrorAsync(ex, @$"Esecuzione EsgUptdTabProd Transaction SQL");
                    tuttoOk = false;
                }
                finally
                {
                    myCommand.Dispose();
                    myTrans.Dispose();
                    if (myConnection != null && myConnection.State == ConnectionState.Open) // Fixed condition
                    {
                        await myConnection.CloseAsync();
                    }
                }                
            }
            return tuttoOk;
        }
    }
}
