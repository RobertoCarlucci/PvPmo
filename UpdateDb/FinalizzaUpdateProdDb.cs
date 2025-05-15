namespace PvPmo.UpdateDb;

public static class FinalizzaUpdateProdDb
{
    public static async Task<bool> ApplicaUptd()
    {
        var repo = new CaricaTabRepository<CaricaTabFinalizza>("pvpmo_origine", "finalizza");
        var tabFinalizza = await repo.GetAllAsync();

        bool tuttoOk = true;

        foreach (var n in tabFinalizza)
        {
            string tabProd = n.TabConfronto ?? "";
            string tabUptd = n.TabTestare ?? "";
            string dbProd = n.DbTabConfronto ?? "";
            string dbUptd = n.DbTabTest ?? "";
            string colMatch = n.ColConfronto ?? "";

            string connProd = Conn.MysqlConn(dbProd);
            string connUptd = Conn.MysqlConn(dbUptd);
            switch (n.Azione)
            {
                case "UPTD":
                    try
                    {
                        if (tabProd is "pv_total" or "global_timesheet_extract")
                        {
                            // 1. Elimina righe dalla produzione dove il DateId coincide
                            string deleteSql = $@"
                        DELETE FROM `{tabProd}` 
                        WHERE `{colMatch}` IN (SELECT DISTINCT `{colMatch}` FROM `{tabUptd}`);";

                            await SqlAsync.SqlNoQry(connProd, deleteSql, 60);

                            // 2. Inserisci le nuove righe dalla tabella di update
                            string insertSql = $@"
                        INSERT INTO `{tabProd}`
                        SELECT * FROM `{dbUptd}`.`{tabUptd}`;";
                            await SqlAsync.SqlNoQry(connProd, insertSql, 180);
                        }
                        else if (tabProd is "all_project_mapped_power_bi_column_set" or "timesheet_information_by_month")
                        {
                            string truncateSql = $"TRUNCATE TABLE `{tabProd}`;";
                            string insertSql = $@"
                        INSERT INTO `{tabProd}`
                        SELECT * FROM `{dbUptd}`.`{tabUptd}`;";

                            await SqlAsync.SqlNoQry(connProd, truncateSql, 30);
                            await SqlAsync.SqlNoQry(connProd, insertSql, 180);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current.DisplayAlert("Errore aggiornamento !", ex.Message, "OK");
                        tuttoOk = false;
                    }
                    break;
            }            
        }        
        return tuttoOk;
    }
}
