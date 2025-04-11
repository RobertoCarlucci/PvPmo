namespace PvPmo.Util;

public class SqlQry
{
    public static async Task<bool> CreaIdMonthYear(string StrConnSql, string nomeTbSql, string nomeCol, string concatString)
    {        
        string QrySql = "UPDATE `" + nomeTbSql + "` SET `" + nomeCol + "` = CONCAT(" + concatString + ");";
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 180);
        return Bol;
    }
    public static async Task<bool> CreaKeyId(string StrConnSql, string nomeTbSql, string nomeCol, string concatString)
    {        
        string QrySql = "UPDATE `" + nomeTbSql + "` SET " + nomeCol + " = CONCAT(" + concatString + ");";
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 180);
        return Bol;
    }
    public static async Task<bool> CreaDateId(string StrConnSql, string nomeTbSql, string nomeCol, string concatString)
    {        
        string QrySql = "UPDATE `" + nomeTbSql + "` SET `" + nomeCol + "` = CONCAT(" + concatString + ");";
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 180);
        return Bol;
    }
    public static async Task<bool> AddColSql(string StrConnSql, string nomeTbSql, string nomeColSql, string sType)
    {        
        string QrySql = "ALTER TABLE `" + nomeTbSql + "` ADD COLUMN `" + nomeColSql + "` " + sType + ";";
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30);
        return Bol;
    }
    public static async Task<bool> DelColSql(string StrConnSql, string nomeTabSql, string nomeColSql)
    {
        string QrySql = "ALTER TABLE `" + nomeTabSql + "` DROP IF EXISTS `" + nomeColSql + "`;";
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30);
        return Bol;
    }
    public static Boolean DelRecSql(string nomeDb, string tabella)
    {
        string StrConn = Conn.MysqlConn(nomeDb);
        string QrySql = "DELETE FROM " + tabella + ";";
        Boolean Bol = SqlSync.SqlNoQry(QrySql, StrConn);
        return Bol;
    }           
    public static async Task<bool> RinColSql(string StrConnSql, string nomeTabSql, string oldNomeColSql, string newwNomeColSql, string typeColSql)
    {        
        string QrySql = "ALTER TABLE `" + nomeTabSql + "` CHANGE `" + oldNomeColSql + "`" +
            " `" + newwNomeColSql + "` " + typeColSql + ";";
        bool Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql, 30);
        return Bol;       
    }
    public static async Task<DataTable> NomiColSql(string StrConnSql, string nomeTbSql, DataTable tabella)
    {        
        string qry = "SHOW COLUMNS FROM `" + nomeTbSql + "`; ";
        await SqlAsync.SqlQryDataTable(StrConnSql, qry, tabella, 60);
        return tabella;
    }    
}
