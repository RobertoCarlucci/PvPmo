namespace PvPmo.ImportDb
{
    public partial class Renata
    {
        public static async Task NewSetup()
        {
            // Apro una finestra di sistema x la selezione della cartella di importazione.
            string _exlPath = await SelCart.PickFolderStatic(default);
            
            bool Bol = await ImportSetup(_exlPath, "Project Cost Performance 2025 Q1", "BC_Database_New", "pvpmo_origine", "Renata");
        }
        public static async Task<bool> ImportSetup(string _exlPath, string nomeFoglioExl, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
        {
            DataTable _tabellaExl = new DataTable();

            string exlPath = _exlPath + "\\" + nomeFoglioExl;
            string StrConnExl = Conn.ExlFileConn(exlPath);
            string StrConnSql = Conn.MysqlConn(nomeDbSql);

            string QryExl = "SELECT * FROM [" + nomeWorkSheet + "$] WHERE 1=0;";
            bool Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabellaExl);
            string QrySql = InpAcsToSql.NormInp(nomeTbSql, _tabellaExl);
            QrySql = "CREATE OR REPLACE TABLE " + QrySql;
            Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30);
            Bol = await CreaMappingExlSql(StrConnExl, StrConnSql, nomeWorkSheet, nomeDbSql, nomeTbSql);
            QryExl = "SELECT * FROM [" + nomeWorkSheet + "$];";
            _tabellaExl = new DataTable();
            Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabellaExl);
            StrConnSql = Conn.MysqlConn(nomeDbSql);
            await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabellaExl, 30);
            return false;
        }
        public static async Task<bool> CreaMappingExlSql(string StrConnExl, string StrConnSql, string nomeWorkSheet, string nomeDbSql, string nomeTbSql)
        {
            DataTable _tabExl = new DataTable();
            DataTable _tabSql = new DataTable();
            string QryExl = "SELECT * FROM [" + nomeWorkSheet + "$] WHERE 1=0;";
            string QrySql = "SHOW COLUMNS FROM `" + nomeTbSql + "`;";

            ExlSync.ExcQry(StrConnExl, QryExl, _tabExl);
            await SqlAsync.SqlQryDataTable(StrConnSql, QrySql, _tabSql, 30);

            int _dif = _tabSql.Rows.Count - _tabExl.Columns.Count;

            if (_dif == 0)
            {
                DataRow[] _rowSql = _tabExl.Select();
                for (int x = 1; x < _tabSql.Rows.Count; x++)
                {
                    int SourceOrdinal = x;
                    string? DestinationColumn = _tabSql.Rows[x]["Field"].ToString();
                    SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
                }
                return true;
            }
            else if (_dif == 1)
            {
                DataRow[] _rowSql = _tabExl.Select();
                for (int x = 1; x < _tabSql.Rows.Count; x++)
                {
                    int SourceOrdinal = x - 1;
                    string? DestinationColumn = _tabSql.Rows[x]["Field"].ToString();
                    SqlAsync.AddMapping(SourceOrdinal, DestinationColumn);
                }
                return true;
            }
            else if (_dif < 0 || _dif > 1)
            {
                var Conf = await Shell.Current.DisplayAlert
                    ("Errore nel numero di Colonne delle Tabelle.", "La Tabella Excel: "
                    + _tabExl + " Ha un numero di colonne diverso dalla tabella Sql: "
                    + _tabSql + " Vuoi continuare ad importare le altre tabelle residue ? "
                    , "Si", "No");
                if (Conf == true)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public static async Task<bool> ExpAcs()
        {
            DataTable _tabExl = new DataTable();
            DataTable _tabSql = new DataTable();

            string nomeDbSql = "pvpmo_origine";
            string nomeTbSql = "Renata";

            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            string QrySql = "SHOW COLUMNS FROM `" + nomeTbSql + "`;";
            await SqlAsync.SqlQryDataTable(StrConnSql, QrySql, _tabSql, 30);
            string QryAcs = NormExpAcs(nomeTbSql, _tabSql);
            QryAcs = "CREATE TABLE " + QryAcs;
            string filePath = await SelCart.PickAndShow(default);

            return true;
        }
        public static string NormExpAcs(string nomeTbAcs, DataTable tabData)
        {
            // Chiamata alla funzione di normalizzazione nome tabella.        

            string Qry = nomeTbAcs + " (";
            int x = 0;
            int i = tabData.Rows.Count;
            DataRow[] temp = tabData.Select();
            foreach (DataRow v in temp)
            {
                string Name = v["Field"].ToString();
                string Type = v["Type"].ToString();
                if (Name == "id")
                {
                    x++;
                }
                else
                { 
                    switch (Type)
                    {
                        case "varchar(50)":
                            Type = ("Text");
                            break;
                        //case "Double":
                        //    Type = ("SMALLINT UNSIGNED");
                        //    break;
                        //case "SMALLINT UNSIGNED":
                        //    Type = ("Int16");
                        //    break;
                        case "smallint(5) unsigned":
                            Type = ("Long");
                            break;
                        case "DATE":
                            Type = ("DateTime");
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
                        case "Modifica":
                            Type = ("nvarchar(120)");
                            break;
                        case "TipoCol":
                            Type = ("nvarchar(120)");
                            break;
                        case "ORE":
                            Type = ("FLOAT");
                            Name = "Ore";
                            break;
                        case "Hours Per Week":
                            Type = ("FLOAT");
                            break;
                        case "Timesheet_Time":
                            Type = ("FLOAT");
                            break;
                        case "Action Items, Completed (#)":
                            Name = "ActionItemsCompletedVal";
                            break;
                        case "Action Items, Completed (%)":
                            Name = "ActionItemsCompletedPerc";
                            break;
                        case "Date":
                            Name = "Dateid";
                            break;
                        case "DateID":
                            Name = "Dateid";
                            break;
                        case "Date ID":
                            Name = "Dateid";
                            break;
                        case "DateKeY":
                            Name = "Datekey";
                            break;
                        case "KeyFTE_Mese":
                            Name = "KeyFteMese";
                            break;
                        case "Key":
                            Name = "Keyid";
                            break;
                        case "KeyID":
                            Name = "Keyid";
                            break;
                        case "ID&Month&Year":
                            Name = "IdMonthYear";
                            break;
                        case "Work ID #":
                            Name = "WorkId";
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
                }
                x++;
            }
            Qry += ");";
            return Qry;
        }
    }
}
