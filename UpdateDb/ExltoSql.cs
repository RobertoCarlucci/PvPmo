namespace PvPmo.UpdateDb
{
    public partial class ExltoSql
    {
        //Importo i File Excel per l'aggiornamento mensile.
        //private static string _cartInput = @"\Archivio\IN\Timesheet\";
        public static async Task InpExl()
        {
            string _exlPath = await SelCart.PickFolderStatic(default);
            ImpExlService _inpexl = new ImpExlService();            

            foreach (var n in _inpexl.FileExl)
            {                
                string nomeFoglio = n.WorkSheet;
                string nomeTbExl = n.tabella;
                string nomeDbSql = n.db_dest;
                string nomeTdDest = n.tabella_sql;
                string exlPath = _exlPath + "\\" + nomeTbExl;
                Boolean Bol = await NomeColFileExltoTabSql(nomeFoglio, nomeDbSql, nomeTdDest, exlPath);
                Bol = await DatiFileExltoTabSql(nomeFoglio, nomeDbSql, nomeTdDest, exlPath);
                switch (nomeTbExl)
                {
                    case "PV_Total_Uptd":
                        Boolean bol = await PvTotalUptd();
                        break;
                    case "GlobalTimesheetExtract_Uptd":
                        bol = await GlobalTimesExtrUptd(nomeDbSql);
                        break;
                }
                //string QrySql = "SHOW COLUMNS FROM " + nomeTdDest;
                //DataTable _showcolumn = new DataTable();
                //await SqlAsync.SqlQryDataTable(StrConnSql, QrySql, _showcolumn);
            }            
        }
        // Se il File è Global Timesheer Extract aggiungo le colonne necessarie e popolo 
        // le stesse con i dati attraverso le opportune query.
        public static async Task<bool> GlobalTimesExtrUptd(string nomeDbSql)
        {
            NormImpService _normimp = new NormImpService();
            foreach (var n in _normimp.NormImp)
            {
                string _colonna = n.colonna;
                string _azione = n.azione;
                string _tabella = n.tabella;
                if (_tabella == "global_timesheet_extract" && _azione == "ADD")
                {
                    Boolean bol = await AddCol(nomeDbSql, _colonna, _tabella);
                    switch (_colonna)
                    {
                        case "DateID":
                            bol = await PvTotalUptd();
                            break;
                        case "id_month_year":
                            bol = await GlobalTimesExtrUptd(nomeDbSql);
                            break;
                        case "keyid":
                            bol = await GlobalTimesExtrUptd(nomeDbSql);
                            break;
                    }
                };
            }            
            return true;
        }        
        // Se il File è PV_Total aggiungo le colonne necessarie e popolo 
        // le stesse con i dati attraverso le opportune query, imoltre rimuovo
        // le collonne che non vengono utilizzate.
        public static async Task<bool> PvTotalUptd()
        {
            return true;
        }
        // Creo i le tabelle nel Db Sql per imortare i dati dai file Excel.
        public static async Task<bool> NomeColFileExltoTabSql(string nomeFoglio, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable _tabella = new DataTable();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string QryExl = "SELECT * FROM [" + nomeFoglio + "$] WHERE 1=0;";
            Boolean Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabella);
            string QrySql = InpExl(nomeTbSql, _tabella);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            QrySql = "CREATE OR REPLACE TABLE " + QrySql;
            Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql);                        
            return true;
        }
        // Inserisco i dati nelle opportune colonne righe
        public static async Task<bool> DatiFileExltoTabSql(string nomeFoglio, string nomeDbSql, string nomeTbSql, string exlPath)
        {
            DataTable _tabella = new DataTable();
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string QryExl = "SELECT * FROM [" + nomeFoglio + "$];";
            Boolean Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabella);
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

