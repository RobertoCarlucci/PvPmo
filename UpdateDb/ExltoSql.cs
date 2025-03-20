namespace PvPmo.UpdateDb
{
    public partial class ExltoSql
    {
        //Importo i File Excel per l'aggiornamento mensile.
        //private static string _cartInput = @"\Archivio\IN\Timesheet\";
        public static async Task InpExl()
        {
            // Apro una finestra di sistema x la selezione della cartella di importazione.
            string _exlPath = await SelCart.PickFolderStatic(default);
            ImpExlService _inpexl = new ImpExlService();            
            Boolean Bol = false;

            foreach (var n in _inpexl.FileExl)
            {                
                string nomeFoglio = n.WorkSheet;
                string nomeTbExl = n.tabella;
                string nomeDbSql = n.db_dest;
                string nomeTdDest = n.tabella_sql;
                string exlPath = _exlPath + "\\" + nomeTbExl;
                Bol = await NomeColFileExltoTabSql(nomeFoglio, nomeDbSql, nomeTdDest, exlPath);
                Bol = await DatiFileExltoTabSql(nomeFoglio, nomeDbSql, nomeTdDest, exlPath);
                
                //string QrySql = "SHOW COLUMNS FROM " + nomeTdDest;
                //DataTable _showcolumn = new DataTable();
                //await SqlAsync.SqlQryDataTable(StrConnSql, QrySql, _showcolumn);
            }
            Bol = await NormTabImp();
        }
        public static async Task<bool> NormTabImp()
        {
            NormImpService _normimp = new NormImpService();
            string _db = "";
            bool Bol = false;
            foreach (var n in _normimp.NormImp)
            {
                string _colonna = n.Colonna;
                string _azione = n.Azione;
                string _tabella = n.Tabella;
                _db = n.Dbdest;
                switch (_tabella)
                {
                    case "pv_total":
                        Bol = await PvTotalUptd(_db, _tabella, _colonna, _azione);
                        break;
                    case "global_timesheet_extract":
                        Bol = await GlobalTimesExtrUptd(_db, _tabella, _colonna, _azione);
                        break;
                }
            }
            string StrConnSql = Conn.MysqlConn(_db);
            string Qry = "DELETE FROM `pv_total`WHERE `Resource Name` = 'Farneti Thomas Old';";
            Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);
            return Bol;
        }
        // Se il File è Global Timesheer Extract aggiungo le colonne necessarie e popolo 
        // le stesse con i dati attraverso le opportune query.
        public static async Task<bool> GlobalTimesExtrUptd(string nomeDbSql, string nomeTab, string nomeCol, string azione)
        {
            if (nomeTab == "global_timesheet_extract" && azione == "ADD")
            {
                switch (nomeCol)
                {
                    case "DateID":
                        string Qry = "ALTER TABLE `" + nomeTab + "` ADD COLUMN `" + nomeCol + "` nvarchar(50);";
                        break;
                    case "id_month_year":
                        Qry = "ALTER TABLE `" + nomeTab + "` ADD COLUMN `" + nomeCol + "` nvarchar(50);";
                        break;
                    case "keyid":
                        Qry = "ALTER TABLE `" + nomeTab + "` ADD COLUMN `" + nomeCol + "` nvarchar(50);";
                        break;
                }                
            }            
            return true;
        }        
        // Se il File è PV_Total aggiungo le colonne necessarie e popolo 
        // le stesse con i dati attraverso le opportune query, imoltre rimuovo
        // le collonne che non vengono utilizzate.
        public static async Task<bool> PvTotalUptd(string nomeDbSql, string nomeTab, string nomeCol, string azione)
        {
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            string Qry = "";
            bool Bol = false;

            if (nomeTab == "pv_total" && azione == "ADD")
            {
                switch (nomeCol)
                {
                    case "DateID":
                        Qry = "ALTER TABLE `" + nomeTab + "` ADD COLUMN `" + nomeCol + "` nvarchar(50);";                        
                        break;
                    case "id_month_year":
                        Qry = "ALTER TABLE `" + nomeTab + "` ADD COLUMN `" + nomeCol + "` nvarchar(50);";                        
                        break;
                    case "keyid":
                        Qry = "ALTER TABLE `" + nomeTab + "` ADD COLUMN `" + nomeCol + "` nvarchar(50);";                        
                        break;
                }
                Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);
            }
            else if (nomeTab == "pv_total" && azione == "DEL")
            {
                Qry = "ALTER TABLE `" + nomeTab + "` DROP IF EXISTS `" + nomeCol + "`;";
                Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);                
            };            
            return Bol;
        }
        // Creo le tabelle nel Db Sql per importare i dati dai file Excel.
        public static async Task<bool> NomeColFileExltoTabSql(string nomeFoglio, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable _tabella = new DataTable();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string QryExl = "SELECT * FROM [" + nomeFoglio + "$] WHERE 1=0;";
            bool Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabella);
            string QrySql = InpExl(nomeTbSql, _tabella);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            QrySql = "CREATE OR REPLACE TABLE " + QrySql;
            Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql);                        
            return true;
        }
        // Inserisco i dati nelle opportune tabelle colonne.
        public static async Task<bool> DatiFileExltoTabSql(string nomeFoglio, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable _tabella = new DataTable();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string QryExl = "SELECT * FROM [" + nomeFoglio + "$];";
            bool Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabella);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella);
            return Bol;            
        }
        // Normalizzo il tipo di dati da importare alle necessità di Sql.
        public static string InpExl(string nomeTabDb, DataTable tabData)
        {
            string Qry = nomeTabDb + " (";
            int x = 0;
            int i = tabData.Columns.Count - 1;

            foreach (DataColumn col in tabData.Columns)
            {
                string Name = col.ColumnName;
                string Type = col.DataType.ToString();
                Type = Type.Remove(0, 7);
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

                if (x < i)
                {
                    Qry = Qry + "`" + Name + "` " + Type + ", ";
                }
                else
                {
                    Qry = Qry + "`" + Name + "` " + Type;
                }
                x++;
            }
            Qry += ");";
            return Qry;
        }
    } 
}

