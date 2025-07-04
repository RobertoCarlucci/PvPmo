namespace PvPmo.Import
{
    public partial class UptdProd
    {
        public static async Task<bool> EsgUptdTabProd(string connProd, string connUptd, IProgress<string>? progress)
        {
            string _connProd = await Conn.MysqlConn(connProd + "; AllowLoadLocalInfile = true");
            string _connUptd = await Conn.MysqlConn(connUptd + "; Convert Zero Datetime = True");

            if (string.IsNullOrEmpty(_connProd) || string.IsNullOrEmpty(_connUptd))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }            

            Dictionary<string, List<MySqlBulkCopyColumnMapping>> _mapsDict;

            string mapPvt = "mapsPvt";
            string mapGtex = "mapsGtex";
            string mapApm = "mapsApm";
            string mapTim = "mapsTim";

            DataTable dataPvt = new();
            DataTable dataGtex = new();
            DataTable dataApm = new();
            DataTable dataTim = new();

            string qryPvt = string.Empty;
            string qryGtex = string.Empty;
            string qryApm = string.Empty;
            string qryTim = string.Empty;

            string nomeTabPvt = string.Empty;
            string nomeTabGtex = string.Empty;
            string nomeTabApm = string.Empty;
            string nomeTabTim = string.Empty;

            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(await Conn.MysqlConn("pvpmo_origine"));

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
                if (string.IsNullOrWhiteSpace(u.DbTabConfronto) || string.IsNullOrWhiteSpace(u.TabConfronto) ||
                    string.IsNullOrWhiteSpace(u.DbTabTest) || string.IsNullOrWhiteSpace(u.TabTestare))
                {
                    await Shell.Current.DisplayAlert("Errore", "Dati per finalizzare l'importazione non completi.", "OK");
                    return false;
                }
                switch (u.TabConfronto)
                {
                    case "all_project_mapped_power_bi_column_set":
                        progress?.Report($"Mapping: {label}");
                        _mapsDict = await DbUtlil.MyMappingList(mapApm, u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto);                        
                        qryApm = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(_connUptd, qryApm, dataApm, 60);
                        if (dataApm.Rows.Count == 0) return false;
                        qryApm = $@"TRUNCATE TABLE `{u.TabConfronto}`;";
                        nomeTabApm = u.TabConfronto;
                        break;
                    case "timesheet_information_by_month":
                        progress?.Report($"Mapping: {label}");
                        _mapsDict = await DbUtlil.MyMappingList(mapTim, u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto);                        
                        qryTim = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(_connUptd, qryTim, dataTim, 60);
                        if (dataTim.Rows.Count == 0) return false;
                        qryTim = $@"TRUNCATE TABLE `{u.TabConfronto}`;";
                        nomeTabTim = u.TabConfronto;
                        break;
                    case "pv_total":                        
                        progress?.Report($"Mapping: {label}");                        
                        _mapsDict = await DbUtlil.MyMappingList(mapPvt, u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto);                        
                        qryPvt = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(_connUptd, qryPvt, dataPvt, 60);
                        if (dataPvt.Rows.Count == 0) return false;
                        qryPvt = $@"DELETE `{u.TabConfronto}`.* FROM `{u.TabConfronto}` INNER JOIN `{u.TabTestare}` ON 
                            `{u.TabConfronto}`.`{u.ColConfronto}` = `{u.TabTestare}`.`{u.ColDaTestare}`;";
                        nomeTabPvt = u.TabConfronto;
                        break;
                    case "global_timesheet_extract":
                        progress?.Report($"Mapping: {label}");                        
                        _mapsDict = await DbUtlil.MyMappingList(mapGtex, u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto);                        
                        qryGtex = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(_connUptd, qryGtex, dataGtex, 60);
                        if (dataGtex.Rows.Count == 0) return false;
                        qryGtex = $@"DELETE `{u.TabConfronto}`.* FROM `{u.TabConfronto}` INNER JOIN `{u.TabTestare}` ON 
                            `{u.TabConfronto}`.`{u.ColConfronto}` = `{u.TabTestare}`.`{u.ColDaTestare}`;";
                        nomeTabGtex = u.TabConfronto;                        
                        break;
                    default:
                        tuttoOk = false;
                        break;
                }

            }
            using (MySqlConnection myConnection = new MySqlConnection(_connProd + "; Convert Zero Datetime=True;"))
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

                    myCommand.CommandTimeout = 60;
                    myCommand.CommandText = qryPvt;
                    progress?.Report($"Clean: Pv_Total");
                    await myCommand.ExecuteNonQueryAsync();

                    myCommand.CommandTimeout = 180;
                    myCommand.CommandText = qryGtex;
                    progress?.Report($"Clean: Global");
                    await myCommand.ExecuteNonQueryAsync();

                    var bulkApm = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = nomeTabApm, BulkCopyTimeout = 30 };
                    var mapsAtm = DbUtlil.ApplicaMappingList(bulkApm, mapApm);
                    if (mapsAtm != null)
                    {
                        mapsAtm.ForEach(bulkApm.ColumnMappings.Add);
                        mapsAtm.Clear(); // This line is safe now because we check for null above
                    }
                    progress?.Report($"Write: All_Projrct");
                    await bulkApm.WriteToServerAsync(dataApm);

                    var bulkTim = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = nomeTabTim, BulkCopyTimeout = 30 };
                    var mapsTim = DbUtlil.ApplicaMappingList(bulkTim, mapTim);
                    if (mapsTim != null)
                    {
                        mapsTim.ForEach(bulkTim.ColumnMappings.Add);
                        mapsTim.Clear(); // This line is safe now because we check for null above
                    }
                    progress?.Report($"Write: Timesheet");
                    await bulkTim.WriteToServerAsync(dataTim);

                    var bulkGtex = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = nomeTabGtex, BulkCopyTimeout = 240 };
                    var mapsGtex = DbUtlil.ApplicaMappingList(bulkGtex, mapGtex);
                    if (mapsGtex != null)
                    {
                        mapsGtex.ForEach(bulkGtex.ColumnMappings.Add);
                        mapsGtex.Clear(); // This line is safe now because we check for null above
                    }
                    progress?.Report($"Write: Global");
                    await bulkGtex.WriteToServerAsync(dataGtex);

                    var bulkPvt = new MySqlBulkCopy(myConnection, myTrans)
                    { DestinationTableName = nomeTabPvt, BulkCopyTimeout = 60 };
                    var mapsPvt = DbUtlil.ApplicaMappingList(bulkPvt, mapPvt);
                    if (mapsPvt != null)
                    {
                        mapsPvt.ForEach(bulkPvt.ColumnMappings.Add);
                        mapsPvt.Clear(); // This line is safe now because we check for null above
                    }
                    progress?.Report($"Write: Pv_Total");
                    await bulkPvt.WriteToServerAsync(dataPvt);

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
                    await myCommand.DisposeAsync();
                    await myTrans.DisposeAsync();
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
