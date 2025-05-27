using DocumentFormat.OpenXml.InkML;

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
}

