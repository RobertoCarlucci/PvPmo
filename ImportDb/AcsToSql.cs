namespace PvPmo.ImportDb
{
    public partial class AcsToSql
    {
        //Importazione dei due Db Access con i loro dati.
        public static async Task NewDb()
        {
            // Apro una finestra di sistema x la selezione della cartella di importazione.
            string _acsPath = await SelCart.PickFolderStatic(default);
            TabAcsService _inpacs = new TabAcsService();
            int x = 1;

            foreach (var n in _inpacs.InpAcs)
            {                
                string? nomeDbAcs = n.DbInp;
                string? nomeTbAcs = n.Tabella;
                string? nomeDbSql = n.DbDest;
                string? tab = await TabAcstoTabSql(nomeDbAcs, nomeTbAcs, nomeDbSql, _acsPath);                
                string StrConnSql = Conn.MysqlConn("pvpmo_origine");
                string Qry = "UPDATE  origine_acs set tabella_sql = '" + tab + "' WHERE id = " + x + ";";
                SqlSync.SqlQry(StrConnSql, Qry);
                x++;
            }
            TabAcsService _inpacsdati = new TabAcsService();

            foreach (var n in _inpacsdati.InpAcs)
            {
                string? nomeDbAcs = n.DbInp;
                string? nomeTbAcs = n.Tabella;
                string? nomeDbSql = n.DbDest;
                string? nomeTbSql = n.TabellaSql;
                await InpAcsDatitoSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, _acsPath);            
            }
        }
        // Creo le tabelle Sql leggendo i nomi delle tabelle Access e normalizzando le
        // intestazioni delle colonne in modo compatibile con sql.
        public static async Task<string> TabAcstoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string acsPath)
        {
            string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0";
            await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
            string nomeTbNorm = NormNomeTab(nomeTbAcs);
            Qry = NormInp(nomeTbAcs, nomeTbNorm, _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;            
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            bool Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);            
            return nomeTbNorm;
        }
        // Importo i dati all'interno del Db andando a popolare con i valori le tabelle
        // colonne precedentemente create.
        public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
        {
            bool Bol = await CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, acsPath);
            if(Bol == true)
            {
                string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
                DataTable _tabella = new DataTable();
                string Qry = "SELECT * FROM [" + nomeTbAcs + "]";
                await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
                string StrConnSql = Conn.MysqlConn(nomeDbSql);
                await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella);
                await AggiungiCol(nomeDbSql, nomeTbSql);
            }
            return Bol;            
        }
        public static async Task<bool> AggiungiCol(string nomeDbSql, string nomeTbSql)
        {
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            string Qry = "";
            bool Bol = true;

            switch (nomeTbSql)
            {
                case "pv_total":
                    Qry = "DELETE FROM `pv_total`WHERE `Resource Name` = 'Farneti Thomas Old';";
                    Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);
                    Qry = "ALTER TABLE `" + nomeTbSql + "` ADD COLUMN `id_month_year` nvarchar(50);";
                    Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);
                    Qry = "ALTER TABLE `" + nomeTbSql + "` ADD COLUMN `keyid` nvarchar(50);";
                    Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);
                    break;
                case "global_timesheet_extract":
                    
                    break;
            }

            return false;
        }
        // Vengono lette le intestazioni delle colonne Access e Sql per creare il mapping
        // delle corrispondenze tra le due e di conseguenza caricato attraverso il metodo
        // AddMapping all'interno della cartella GestDb classe gestione Sql.
        public static async Task<bool> CreaMappingAcsSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
        {
            DataTable _tabAcs = new DataTable();
            DataTable _tabSql = new DataTable();
            string QryAcs = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0;";
            string QrySql = "SELECT * FROM `information_schema`.`COLUMNS` " +
                "WHERE TABLE_SCHEMA = '" + nomeDbSql + "' AND TABLE_NAME = " +
                "'" + nomeTbSql + "' ORDER BY ORDINAL_POSITION;";

            string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);

            await AcsAsync.AcsQryTab(StrConnAcs, QryAcs, _tabAcs);
            await SqlAsync.SqlQryDataReader(StrConnSql, QrySql, _tabSql);

            int _dif = _tabSql.Rows.Count - _tabAcs.Columns.Count;

            if (_dif == 0)
            {
                DataRow[] _rowSql = _tabAcs.Select();
                for (int x = 1; x < _tabSql.Rows.Count; x++)
                {
                    int SourceOrdinal = x;
                    string? DestinationColumn = _tabSql.Rows[x]["COLUMN_NAME"].ToString();
                    SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
                }
                return true;
            }
            else if (_dif == 1)
            {
                DataRow[] _rowSql = _tabAcs.Select();
                for (int x = 1; x < _tabSql.Rows.Count; x++)
                {
                    int SourceOrdinal = x - 1;
                    string? DestinationColumn = _tabSql.Rows[x]["COLUMN_NAME"].ToString();
                    SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
                }
                return true;
            }
            else if (_dif < 0 || _dif > 1)
            {
                var Conf = await Shell.Current.DisplayAlert
                    ("Errore nel numero di Colonne delle Tabelle.", "La Tabella Access: "
                    + _tabAcs + " Ha un numero di colonne diverso dalla tabella Sql: "
                    + _tabSql + " Vuoi continuare ad importare le altre tabelle residue ? "
                    , "Si", "No");
                if (Conf == true)
                    return true;
                else

                    return false;
            }
            return false;
        }
        // Vengono normalizzati i nomi delle Tabelle Sql creando le stesse.
        // Si procede anche alla normalizzazione dei nomi colonna.
        public static string NormInp(string nomeTabDb, string nomeTabNorm, DataTable tabData)
        {
            // Chiamata alla funzione di normalizzazione nome tabella.
            nomeTabDb = nomeTabNorm;

            string Qry = nomeTabDb + " (";
            int x = 0;
            int i = tabData.Columns.Count - 1;

            foreach (DataColumn col in tabData.Columns)
            {
                string Name = col.ColumnName;
                string Type = col.DataType.ToString();
                Type = Type.Remove(0, 7);
                // Aggiungo colonna id se non presente.
                if (x == 0 && Name != "ID")
                {
                    string idType = ("INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
                    string idName = "id";
                    Qry = Qry + "`" + idName + "` " + idType + ", ";
                }
                // Select tipo dati colonne.
                switch (Type)
                {
                    case "String":
                        Type = ("nvarchar(50)");
                        break;
                    case "Double":
                        Type = ("SMALLINT UNSIGNED");
                        break;
                    case "Int16":
                        Type = ("SMALLINT UNSIGNED");
                        break;
                    case "Single":
                        Type = ("SMALLINT UNSIGNED");
                        break;
                    case "DateTime":
                        Type = ("DATE NOT NULL DEFAULT '0001-01-01'");
                        break;
                }
                // Select nomi colonne e creazione chiave primaria dove necessaria.
                switch (Name)
                {
                    case "ID":
                        Type = ("INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT");
                        Name = "id";
                        break;
                    case "Divisore":
                        Type = ("FLOAT");
                        break;
                    case "ORE":
                        Type = ("FLOAT");
                        break;
                    case "Hours Per Week":
                        Type = ("FLOAT");
                        break;
                    case "Timesheet_Time":
                        Type = ("FLOAT");
                        break;
                    case "Action Items, Completed (#)":
                        Name = "action_items_completed_val";
                        break;
                    case "Action Items, Completed (%)":
                        Name = "action_items_completed_perc";
                        break;
                    case "Date":
                        Name = "dateid";
                        break;
                    case "Key":
                        Name = "keyid";
                        break;
                    case "ID&Month&Year":
                        Name = "id_month_year";
                        break;
                    case "Work ID #":
                        Name = "work_id";
                        break;
                }
                // Eliminazione dei caratteri speciali possibili in access.
                string RemVirgola = Name.Replace(",", "");
                string RemTrattino = RemVirgola.Replace("-", "_");
                string RemSpazi = RemTrattino.Replace(" ", "_");
                string RemTondeIn = RemSpazi.Replace("(", "_");
                string RemTondeFn = RemTondeIn.Replace(")", "_");
                // Accodamento nella query dei nomi campi.
                // Viene usato il carattere ` (Alt + 96) per indicare tipo stringa nel
                // nome colonna.
                if (x < i)
                {
                    Qry = Qry + "`" + RemTondeFn + "` " + Type + ", ";
                }
                else
                {
                    Qry = Qry + "`" + RemTondeFn + "` " + Type;
                }
                x++;
            }
            Qry += ");";
            return Qry;
        }
        // Vengono normalizzati i nomi delle Tabelle Sql.
        public static string NormNomeTab(string nomeTab)
        {
            switch (nomeTab)
            {
                case "01_TabellaData":
                    nomeTab = "tabella_data";
                    break;
                case "All Project Mapped - Power BI Column Set":
                    nomeTab = "all_project_mapped_power_bi_column_set";
                    break;
                case "PBX_ AllProjectMappedPowerBIColumnSet":
                    nomeTab = "pbx_all_project_mapped_power_bi_column_set";
                    break;
                case "Resource Type":
                    nomeTab = "resource_type";
                    break;
                case "ScenarioRestoAnno":
                    nomeTab = "scenario_resto_anno";
                    break;
                case "Standard Activities":
                    nomeTab = "standard_activities";
                    break;
                case "Timesheet Information By Month":
                    nomeTab = "timesheet_information_by_month";
                    break;
                case "Working Hours by Day":
                    nomeTab = "working_hours_by_day";
                    break;
                case "GlobalTimesheetExtract":
                    nomeTab = "global_timesheet_extract";
                    break;
                case "Key":
                    nomeTab = "key_global";
                    break;
                case "PBX_TimesheetInformationByMonth":
                    nomeTab = "pbx_timesheet_information_by_month";
                    break;                
                case "pv_total_outsoremese":
                    nomeTab = "pv_total_outs_ore_mese";
                    break;
            }
            return nomeTab;
        }
    }
}
