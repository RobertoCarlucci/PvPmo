using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2013.Excel;

namespace PvPmo.UpdateDb
{
    public partial class UptdProd
    {
        public static async Task<bool> EsgUptdTabProd(string connProd, string connUptd, IProgress<string>? progress)
        {
            string connProd = Conn.MysqlConn(connProd + ";AllowLoadLocalInfile=true");
            string connUptd = Conn.MysqlConn(connUptd + ";Convert Zero Datetime=True");

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

            var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(Conn.MysqlConn("pvpmo_origine"));

            //    // Carico il File dall'archivio con la sequenza da svolgere
            //    // e provvedo all'esecuzione.

            var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
            var dati = await repo.GetAllAsync();

            bool tuttoOk = true;

            // Se non ci sono tabelle da testare esco.
            // Eseguo i test sui file exl di update importati in pmo_origine.
            bool uptdOk = true;
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
                        await SqlAsync.SqlQryDataTable(connUptd, qryApm, dataApm, 60);
                        if (dataApm.Rows.Count == 0) return false;
                        qryApm = $@"TRUNCATE TABLE `{u.TabConfronto}`;";
                        break;
                    case "timesheet_information_by_month":
                        progress?.Report($"Mapping: {label}");
                        MappingsTim = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsTim == null) return false;
                        qryTim = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(connUptd, qryApm, dataTim, 60);
                        if (dataApm.Rows.Count == 0) return false;
                        qryTim = $@"TRUNCATE TABLE `{u.TabConfronto}`;";
                        break;
                    case "pv_total":
                        progress?.Report($"Mapping: {label}");
                        MappingsPvt = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsPvt == null) return false;
                        qryPvt = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(connUptd, qryPvt, dataPvt, 60);
                        if (dataPvt.Rows.Count == 0) return false;
                        qryPvt = $@"DELETE `{u.TabConfronto}`.* FROM `{u.TabConfronto}` INNER JOIN `{u.TabTestare}` ON 
                            `{u.TabConfronto}`.`{u.ColConfronto}` = `{u.TabTestare}`.`{u.ColDaTestare}`;";
                        break;
                    case "global_timesheet_extract":
                        progress?.Report($"Mapping: {label}");
                        MappingsGtex = await DbUtlil.MyMapping("SQL", u.DbTabConfronto, u.TabConfronto, u.DbTabTest, u.TabConfronto, null, null);
                        if (MappingsGtex == null) return false;
                        qryGtex = $@"SELECT * FROM `{u.TabConfronto}`;";
                        progress?.Report($"Load: {label}");
                        await SqlAsync.SqlQryDataTable(connUptd, qryGtex, dataGtex, 60);
                        if (dataGtex.Rows.Count == 0) return false;
                        qryGtex = $@"DELETE `{u.TabConfronto}`.* FROM `{u.TabConfronto}` INNER JOIN `{u.TabTestare}` ON 
                            `{u.TabConfronto}`.`{u.ColConfronto}` = `{u.TabTestare}`.`{u.ColDaTestare}`;";
                        break;
                    default:
                        tuttoOk = false;
                        break;
                }
                tuttoOk = false; // Se almeno un test fallisce, non procedo con l'aggiornamento.
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
                    progress?.Report($"Clean: {label}");
                    await myCommand.ExecuteNonQueryAsync();
                    myCommand.CommandText = qryTim;
                    progress?.Report($"Clean: {label}");
                    await myCommand.ExecuteNonQueryAsync();
                    myCommand.CommandText = qryPvt;
                    progress?.Report($"Clean: {label}");
                    await myCommand.ExecuteNonQueryAsync();
                    myCommand.CommandText = qryGtex;
                    progress?.Report($"Clean: {label}");
                    await myCommand.ExecuteNonQueryAsync();
                    progress?.Report($"Write: {label}");
                    var bulk = new MySqlBulkCopy(myConnection, myTrans)
                    {
                        DestinationTableName = tabConfronto,
                        BulkCopyTimeout = 240
                    };
                    if (MappingsApm != null)
                    {
                        MappingsApm.ForEach(_mapping => { bulk.ColumnMappings.Add(_mapping); });
                        MappingsApm.Clear(); // This line is safe now because we check for null above
                    }
                    await bulk.WriteToServerAsync(dataApm);
                    await myTrans.CommitAsync();
                }
                catch (MySqlException ex)
                {
                    await myTrans.RollbackAsync();
                    await DbErrorHandler.ShowErrorAsync(ex, @$"Esecuzione EsgUptdTabProd '{tabConfronto}' Transaction SQL");
                    return false;
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
                return true;
            }            
        }
    }
}
