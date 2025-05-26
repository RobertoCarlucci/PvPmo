using DocumentFormat.OpenXml.InkML;

namespace PvPmo.UpdateDb;

public static class FinalizzaUpdateProdDb
{
    public static async Task<bool> ApplicaUptd(string type)
    {
        var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
        var dati = await repo.GetAllAsync();
        
        bool tuttoOk = true;

        var validi = dati.Where(n => n.Azione == type);

        foreach (var n in validi)
        {            
            string connProd = Conn.MysqlConn(n.DbTabConfronto + ";Convert Zero Datetime=True");                        
            string connUptd = Conn.MysqlConn(n.DbTabTest + ";Convert Zero Datetime=True");                        

            if (n.TabConfronto is "pv_total" or "global_timesheet_extract")
            {
                List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

                // 1. Elimina righe dalla produzione dove il DateId coincide
                string deleteSql = $@"
                DELETE `{n.TabConfronto}`.* FROM `{n.TabConfronto}` INNER JOIN `{n.TabTestare}` ON
                `{n.TabConfronto}`.`{n.ColConfronto}` = `{n.TabTestare}`.`{n.ColDaTestare}`;";
                bool delOk = await SqlAsync.SqlNoQry(connProd, deleteSql, 60);

                // 2. Crea Mapping tra le colonne della tabella di produzione e quella di update.
                Mappings = await NormTab.CreaMapping("SQL", "NotUsed", "NotUsed", "NotUsed", n.DbTabTest, n.TabConfronto, "NotUsed");

                // 3. Leggi tabella da importare
                string loadSql = $@"SELECT * FROM `{n.TabConfronto}`;";
                DataTable dt = new DataTable();
                await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);

                // 4. Inserisci le nuove righe dalla tabella di update                        
                bool bulkOk = await SqlAsync.SqlBulkCopy(connProd, n.TabConfronto, dt, Mappings, 180);
            }
            else if (n.TabConfronto is "all_project_mapped_power_bi_column_set" or "timesheet_information_by_month")
            {
                List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

                // 1. Crea Mapping tra le colonne della tabella di produzione e quella di update.
                Mappings = await NormTab.CreaMapping("SQL", "NotUsed", "NotUsed", "NotUsed", n.DbTabTest, n.TabConfronto, "NotUsed");

                // 2. Leggi tabella da importare
                string loadSql = $@"SELECT * FROM `{n.TabConfronto}`;";
                DataTable dt = new DataTable();
                await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);

                // 3. Cancella i dati esistenti nella tabella di produzione.
                string truncateSql = $"TRUNCATE TABLE `{n.TabConfronto}`;";
                await SqlAsync.SqlNoQry(connProd, truncateSql, 30);

                // 4. Inserisci le nuove righe dalla tabella di update
                await SqlAsync.SqlBulkCopy(connProd, n.TabConfronto, dt, Mappings, 180);
            }      
        }
        return tuttoOk;
    }       
}

