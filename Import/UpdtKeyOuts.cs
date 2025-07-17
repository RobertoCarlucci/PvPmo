namespace PvPmo.Import;

public static class UpdtKeyOuts
{    
    public static async Task<bool> UptdKeyOutsTransaction(string myConnString, IProgress<string>? progress)
    {
        string _connProd = string.Empty;

        _connProd = (!string.IsNullOrEmpty(myConnString)) ? _connProd = await Conn.MysqlConn(myConnString) : _connProd;
        if (string.IsNullOrEmpty(_connProd))
        {
            await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
            return false;
        }

        using (MySqlConnection myConnection = new MySqlConnection(_connProd))
        {
            await myConnection.OpenAsync();
            // Start a local transaction
            MySqlTransaction myTrans = myConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            MySqlCommand myCommand = myConnection.CreateCommand();
            myCommand.Transaction = myTrans;
            try
            {
                myCommand.CommandText = "DROP TABLE IF EXISTS `key_global`;";                
                progress?.Report($"Clean: Elimina Tabella key_global");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "CREATE TABLE key_global (id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, Org VARCHAR(50), SubOrg VARCHAR(50)," +
                    "Team VARCHAR(50), Competence VARCHAR(50), LocationRegion VARCHAR(50), Keyid VARCHAR(100));";
                progress?.Report($"Write: Crea Tabella key_global");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "INSERT INTO key_global(Org, SubOrg, Team, Competence, LocationRegion, Keyid) SELECT DISTINCT " +
                    "OrganizationOBS, SubOrgOBS, TeamOBS, CompetencePrimaryValue, JobLocationRegion, Keyid  FROM pmo.pv_total " +
                    "WHERE pv_total.OrganizationOBS IS NOT NULL AND pv_total.SubOrgOBS IS NOT NULL AND pv_total.TeamOBS IS NOT NULL " +
                    "AND pv_total.CompetencePrimaryValue IS NOT NULL AND pv_total.JobLocationRegion IS NOT NULL AND pv_total.Keyid IS NOT NULL ;";
                progress?.Report($"Write: Inserisci dati Tabella key_global");                
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "INSERT INTO key_global(Org, SubOrg, Team, Competence, LocationRegion, Keyid) SELECT DISTINCT " +
                    "Org, SubOrg, Team, Competence, LocationRegion, Keyid  FROM pmo.global_timesheet_extract " +
                    "WHERE global_timesheet_extract.Org IS NOT NULL AND global_timesheet_extract.SubOrg IS NOT NULL AND global_timesheet_extract.Team IS NOT NULL " +
                    "AND global_timesheet_extract.Competence IS NOT NULL AND global_timesheet_extract.LocationRegion IS NOT NULL AND global_timesheet_extract.Keyid IS NOT NULL;";
                progress?.Report($"Write: Inserisci dati Tabella key_global");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "DROP TABLE IF EXISTS `pbx_key`;";
                progress?.Report($"Clean: Elimina Tabella pbx_key");
                await myCommand.ExecuteNonQueryAsync(); 
                myCommand.CommandText = "CREATE TABLE pbx_key (id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, Org VARCHAR(50), SubOrg VARCHAR(50)," +
                    "Team VARCHAR(50), Competence VARCHAR(50), LocationRegion VARCHAR(50), Keyid VARCHAR(100))";
                progress?.Report($"Write: Crea Tabella pbx_key");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "INSERT INTO pbx_key(Org, SubOrg, Team, Competence, LocationRegion, Keyid) SELECT DISTINCT " +
                    "Org, SubOrg, Team, Competence, LocationRegion, Keyid  FROM pmo.key_global WHERE key_global.Keyid IS NOT NULL;";
                progress?.Report($"Write: Inserisci dati Tabella pbx_key");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "DROP TABLE IF EXISTS outs_ore_mese;";
                progress?.Report($"Clean: Elimina Tabella outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "CREATE TABLE outs_ore_mese (id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, GEC VARCHAR(50), " +
                    "timesheetyear SMALLINT, timesheetmonth SMALLINT, Dateid DATE, ORE FLOAT);";
                progress?.Report($"Write: Crea Tabella outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "INSERT INTO outs_ore_mese (GEC, timesheetyear, timesheetmonth, Dateid, ORE) SELECT GEC, YEAR(TimesheetDate), " +
                    "MONTH(TimesheetDate), STR_TO_DATE(CONCAT(YEAR(TimesheetDate), '-', LPAD(MONTH(TimesheetDate), 2, '0'), '-01'), '%Y-%m-%d'), " +
                    "SUM(TimesheetTime) FROM global_timesheet_extract WHERE GEC LIKE '9%' AND Organization = 'Outsourcing' " +
                    "GROUP BY GEC, timesheetyear, timesheetmonth;";
                progress?.Report($"Struct: Inserisci dati Tabella outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "DROP TABLE IF EXISTS pv_total_outs;";
                progress?.Report($"Clean: Elimina Tabella pv_total_outs");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "CREATE TABLE pv_total_outs (id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY, SubOrgOBS VARCHAR(50), " +
                    "CompetencePrimaryValue VARCHAR(50), JobLocationRegion VARCHAR(50), Dateid DATE, ResourceTypes VARCHAR(50), ResourceName VARCHAR(50), " +
                    "JobLocationCountry VARCHAR(50), TeamOBS VARCHAR(50), WorkLocation VARCHAR(50), ShortName VARCHAR(50));";
                progress?.Report($"Crate: Crea Tabella pv_total_outs");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "INSERT INTO pv_total_outs (SubOrgOBS, CompetencePrimaryValue, JobLocationRegion, Dateid, ResourceTypes, " +
                    "ResourceName, JobLocationCountry, TeamOBS, WorkLocation, ShortName) SELECT pv_total.SubOrgOBS, pv_total.CompetencePrimaryValue, " +
                    "pv_total.JobLocationRegion, pv_total.Dateid, pv_total.ResourceTypes, pv_total.ResourceName, pv_total.JobLocationCountry, " +
                    "pv_total.TeamOBS, pv_total.WorkLocation, pv_total.ShortName FROM pmo.pv_total WHERE pv_total.ShortName LIKE '9%' " +
                    "AND pv_total.OrganizationOBS = 'Outsourcing';";
                progress?.Report($"Write: Inserisci dati Tabella pv_total_outs");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "ALTER TABLE `pv_total_outs` ADD COLUMN `VersionName` VARCHAR(50) DEFAULT 'Actual FTE';";
                progress?.Report($"Struct: Modifica Tabella pv_total_outs");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "CREATE OR REPLACE TABLE pv_total_outs_ore_mese SELECT Pv_Total_Outs.*, Outs_Ore_Mese.ORE, " +
                    "Outs_Ore_Mese.timesheetyear, Outs_Ore_Mese.timesheetmonth FROM PV_Total_Outs INNER JOIN Outs_Ore_Mese " +
                    "ON (PV_Total_Outs.DateId = Outs_Ore_Mese.DateId) AND (PV_Total_Outs.ShortName = Outs_Ore_Mese.GEC);";
                progress?.Report($"Create: Crea Tabella pv_total_outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "ALTER TABLE pv_total_outs_ore_mese DROP COLUMN id";
                progress?.Report($"Struct: Modifica Tabella pv_total_outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "ALTER TABLE pv_total_outs_ore_mese ADD COLUMN id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY FIRST;";
                progress?.Report($"Struct: Modifica Tabella pv_total_outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "ALTER TABLE pv_total_outs_ore_mese ADD COLUMN `KeyFteMese` VARCHAR(50);";
                progress?.Report($"Struct: Modifica Tabella pv_total_outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();
                myCommand.CommandText = "UPDATE pv_total_outs_ore_mese SET KeyFteMese = CONCAT(timesheetyear, LPAD(timesheetmonth, 2, '0'));";
                progress?.Report($"Write: Inserisci dati Tabella pv_total_outs_ore_mese");
                await myCommand.ExecuteNonQueryAsync();                
                await myTrans.CommitAsync();                
            }
            catch (MySqlException ex)
            {
                await myTrans.RollbackAsync();
                await DbErrorHandler.ShowErrorAsync(ex, "Esecuzione UptdKeyOuts Transaction SQL");                
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