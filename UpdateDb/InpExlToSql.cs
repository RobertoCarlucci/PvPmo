using PvPmo.Service;

namespace PvPmo.UpdateDb;

public partial class InpExlToSql
{
    //Importo i File Excel per l'aggiornamento mensile.
    
    public static async Task InpExl()
    {
        // Apro una finestra di sistema x la selezione della cartella di importazione.
        string _exlPath = await SelCart.PickFolderStatic(default);
        CaricaTabOriginiService _leggitabdborigine = new CaricaTabOriginiService();
        bool Bol = false;

        foreach (var n in _leggitabdborigine.CaricaTabOrigini)
        {
            string? nomeDbAcs = n.DbInp;
            string? nomeTb = n.Tabella;
            string? nomeDbSql = n.DbDest;
            string? nomeTabSql = n.TabellaSql;
            string? nomeWorkSheet = n.WorkSheet;
            string? inpType = n.InpType;
            if (inpType == "EXL")
            {
                string? exlPath = _exlPath + "\\" + nomeTb;
                Bol = await NomeColFileExltoTabSql(nomeWorkSheet, nomeDbSql, nomeTabSql, exlPath);
                Bol = await DatiFileExltoTabSql(nomeWorkSheet, nomeDbSql, nomeTabSql, exlPath);
            }                        
        }
        Bol= await NormTab.NormTabImp();
    }
    
    // Creo le tabelle nel Db Sql per importare i dati dai file Excel.
    public static async Task<bool> NomeColFileExltoTabSql(string nomeFoglioExl, string nomeDbSql, string nomeTbSql, string exlPath)
    {
        DataTable _tabellaExl = new DataTable();        
        string StrConnExl = Conn.ExlFileConn(exlPath);
        string StrConnSql = Conn.MysqlConn(nomeDbSql);
        string QryExl = "SELECT * FROM [" + nomeFoglioExl + "$] WHERE 1=0;";        
        bool Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabellaExl);
        string QrySql = InpExl(nomeTbSql, _tabellaExl);        
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

