using Windows.Media.AppBroadcasting;

namespace PvPmo.UpdateDb;

public static class FinalUpdtProdDb
{
    public static async Task<bool> ApplicaUptd(string type, string label, IProgress<string>? progress)
    {
        var descrizioni = await DescrizioniProgress.GetProgressDescriptionsAsync(Conn.MysqlConn("pvpmo_origine"));

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
                progress?.Report($"Struct: {label}");
                tuttoOk = await SqlAsync.SqlNoQry(connProd, deleteSql, 60);
                if (!tuttoOk) return false;

                // 2. Crea Mapping tra le colonne della tabella di produzione e quella di update.
                progress?.Report($"Mapping: {label}");
                Mappings = await NormTab.CreaMapping("SQL", "NotUsed", "NotUsed", "NotUsed", n.DbTabTest, n.TabConfronto, "NotUsed");

                // 3. Leggi tabella da importare
                string loadSql = $@"SELECT * FROM `{n.TabConfronto}`;";
                DataTable dt = new DataTable();
                progress?.Report($"Load: {label}");
                await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);
                if (dt.Rows.Count == 0) return false;
                
                // 4. Inserisci le nuove righe dalla tabella di update
                progress?.Report($"Write: {label}");
                tuttoOk = await SqlAsync.SqlBulkCopy(connProd, n.TabConfronto, dt, Mappings, 180);
                if (!tuttoOk) return false;
                
            }
            else if (n.TabConfronto is "all_project_mapped_power_bi_column_set" or "timesheet_information_by_month")
            {
                List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();

                // 1. Crea Mapping tra le colonne della tabella di produzione e quella di update.
                progress?.Report($"Mapping: {label}");
                Mappings = await NormTab.CreaMapping("SQL", "NotUsed", "NotUsed", "NotUsed", n.DbTabTest, n.TabConfronto, "NotUsed");
                if (Mappings == null) return false;

                // 2. Leggi tabella da importare
                string loadSql = $@"SELECT * FROM `{n.TabConfronto}`;";
                DataTable dt = new DataTable();
                progress?.Report($"Load: {label}");
                await SqlAsync.SqlQryDataTable(connUptd, loadSql, dt, 60);
                if (dt.Rows.Count == 0) return false;

                // 3. Cancella i dati esistenti nella tabella di produzione.
                string truncateSql = $"TRUNCATE TABLE `{n.TabConfronto}`;";
                progress?.Report($"Struct: {label}");
                tuttoOk = await SqlAsync.SqlNoQry(connProd, truncateSql, 60);
                if (!tuttoOk) return false;

                // 4. Inserisci le nuove righe dalla tabella di update
                progress?.Report($"Write: {label}");
                tuttoOk = await SqlAsync.SqlBulkCopy(connProd, n.TabConfronto, dt, Mappings, 180);
                if (!tuttoOk) return false;

                if (n.TabConfronto is "all_project_mapped_power_bi_column_set" && tuttoOk is true) 
                {
                    string cancSql = $"DELETE FROM all_project_mapped_power_bi_column_set WHERE SequenceID IS NULL;";
                    progress?.Report($"Struct: {label}");
                    tuttoOk = await SqlAsync.SqlNoQry(connProd, cancSql, 60);
                    if (!tuttoOk) return false;
                }

            }      
        }
        return tuttoOk;
    }
    public static async Task<bool> UptdKey(IProgress<string>? progress)
    {
        string connProd = Conn.MysqlConn("pmo");
        bool keyOk;

        // 1. Cancella i dati esistenti nella tabella di produzione.
        string truncateSql = $"TRUNCATE TABLE `key_global`;";
        progress?.Report($"Struct: Create Key Id");
        keyOk = await SqlAsync.SqlNoQry(connProd, truncateSql, 60);
        if (!keyOk) return  false;

        // 2. Inserisci colonne tabella global timesheet extract X chiave
        string loadSql = $@"INSERT INTO key_global(Org, SubOrg, Team, Competence, LocationRegion, Keyid) SELECT DISTINCT 
            OrganizationOBS, SubOrgOBS, TeamOBS, CompetencePrimaryValue, JobLocationRegion, Keyid  FROM pmo.pv_total;";
        progress?.Report($"Load: Key Id");
        keyOk = await SqlAsync.SqlNoQry(connProd, loadSql,60);
        if (!keyOk) return false;

        // 3. Inserisci colonne tabella global timesheet extract X chiave
        loadSql = $@"INSERT INTO key_global(Org, SubOrg, Team, Competence, LocationRegion, Keyid) SELECT DISTINCT 
            Org, SubOrg, Team, Competence, LocationRegion, Keyid  FROM pmo.global_timesheet_extract;";        
        progress?.Report($"Load: Key Id");
        keyOk = await SqlAsync.SqlNoQry(connProd, loadSql, 60);
        if (!keyOk) return false;

        // 4. Cancella i dati esistenti nella tabella di produzione.
        truncateSql = $"TRUNCATE TABLE `pbx_key`;";
        progress?.Report($"Struct: Create Key Id");
        keyOk = await SqlAsync.SqlNoQry(connProd, truncateSql, 60);
        if (!keyOk) return false;

        // 5. Inserisci colonne tabella key_global pulita di eventuali null o doppioni
        loadSql = $@"INSERT INTO pbx_key(Org, SubOrg, Team, Competence, LocationRegion, Keyid) SELECT DISTINCT 
            Org, SubOrg, Team, Competence, LocationRegion, Keyid  FROM pmo.key_global WHERE key_global.Keyid IS NOT NULL;";
        progress?.Report($"Load: Key Id");
        keyOk = await SqlAsync.SqlNoQry(connProd, loadSql, 60);
        if (!keyOk) return false;

        return keyOk;
    }
    public static async Task<bool> UptdOuts(string conn, IProgress<string>? progress)
    {
        string connProd = Conn.MysqlConn(conn);        

        string dropSql = $@"DROP TABLE IF EXISTS outs_ore_mese;";
        bool dropSqlOk = await SqlAsync.SqlNoQry(connProd, dropSql, 10, null);
        if (!dropSqlOk) return false;

        string createSql = $@"CREATE TABLE outs_ore_mese (id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, GEC VARCHAR(50),
            timesheetyear INT, timesheetmonth INT, Dateid DATE, ORE FLOAT);";
        await SqlAsync.SqlNoQry(connProd, createSql, 10, null);

        string insSql = $@"INSERT INTO outs_ore_mese (GEC, timesheetyear, timesheetmonth, Dateid, ORE) SELECT GEC, 
            YEAR(TimesheetDate), MONTH(TimesheetDate), STR_TO_DATE(CONCAT(YEAR(TimesheetDate), '-', LPAD(MONTH(TimesheetDate), 2, '0'), 
            '-01'), '%Y-%m-%d'), SUM(TimesheetTime) FROM global_timesheet_extract WHERE GEC LIKE '9%' 
            AND Organization = 'Outsourcing' GROUP BY GEC, timesheetyear, timesheetmonth;";
        bool insSqlOk = await SqlAsync.SqlNoQry(connProd, insSql, 60, null);
        if (!insSqlOk) return false;

        dropSql = $@"DROP TABLE IF EXISTS pv_total_outs;";
        dropSqlOk =await SqlAsync.SqlNoQry(connProd, dropSql, 10, null);
        if (!dropSqlOk) return false;

        createSql = $@"CREATE TABLE pv_total_outs (id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, 
            SubOrgOBS VARCHAR(50), CompetencePrimaryValue VARCHAR(50), JobLocationRegion VARCHAR(50), Dateid DATE, 
            ResourceTypes VARCHAR(50), ResourceName VARCHAR(50), JobLocationCountry VARCHAR(50), TeamOBS VARCHAR(50), 
            WorkLocation VARCHAR(50), ShortName VARCHAR(50));";
        bool createSqlOk = await SqlAsync.SqlNoQry(connProd, createSql, 10, null);
        if (!createSqlOk) return false;

        insSql = $@"INSERT INTO pv_total_outs (SubOrgOBS, CompetencePrimaryValue, JobLocationRegion, Dateid, 
            ResourceTypes, ResourceName, JobLocationCountry, TeamOBS, WorkLocation, ShortName) SELECT pv_total.SubOrgOBS, 
            pv_total.CompetencePrimaryValue, pv_total.JobLocationRegion, pv_total.Dateid, pv_total.ResourceTypes, 
            pv_total.ResourceName, pv_total.JobLocationCountry, pv_total.TeamOBS, pv_total.WorkLocation, pv_total.ShortName 
            FROM pmo.pv_total WHERE pv_total.ShortName LIKE '9%' AND pv_total.OrganizationOBS = 'Outsourcing';";
        insSqlOk = await SqlAsync.SqlNoQry(connProd, insSql, 60, null);
        if (!insSqlOk) return false;

        string alterSql = $@"ALTER TABLE `pv_total_outs` ADD COLUMN `VersionName` VARCHAR(50) DEFAULT 'Actual FTE';";
        bool alterSqlOk = await SqlAsync.SqlNoQry(connProd, alterSql, 10, null);
        if (!alterSqlOk) return false;

        createSql = $@"CREATE OR REPLACE TABLE pv_total_outs_ore_mese SELECT Pv_Total_Outs.*,
            Outs_Ore_Mese.ORE, Outs_Ore_Mese.timesheetyear, Outs_Ore_Mese.timesheetmonth FROM PV_Total_Outs INNER JOIN 
            Outs_Ore_Mese ON (PV_Total_Outs.DateId = Outs_Ore_Mese.DateId) AND (PV_Total_Outs.ShortName = Outs_Ore_Mese.GEC);";
        createSqlOk = await SqlAsync.SqlNoQry(connProd, createSql, 10, null);
        if (!createSqlOk) return false;

        alterSql = $@"ALTER TABLE pv_total_outs_ore_mese DROP COLUMN id;";
        alterSqlOk = await SqlAsync.SqlNoQry(connProd, alterSql, 10, null);
        if (!alterSqlOk) return false;

        alterSql = $@"ALTER TABLE pv_total_outs_ore_mese ADD COLUMN id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST;";
        alterSqlOk = await SqlAsync.SqlNoQry(connProd, alterSql, 10, null);
        if (!alterSqlOk) return false;

        alterSql = $@"ALTER TABLE pv_total_outs_ore_mese ADD COLUMN `KeyFteMese` VARCHAR(50);";
        alterSqlOk = await SqlAsync.SqlNoQry(connProd, alterSql, 10, null);
        if (!alterSqlOk) return false;

        string updateSql = $@"UPDATE pv_total_outs_ore_mese SET KeyFteMese = CONCAT(timesheetyear, LPAD(timesheetmonth, 2, '0'));";
        bool updateSqlOk = await SqlAsync.SqlNoQry(connProd, updateSql, 10, null);
        if (!updateSqlOk) return false;

        return true;
    }
}

