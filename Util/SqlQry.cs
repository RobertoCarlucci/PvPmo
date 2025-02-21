using PvPmo.Db;

namespace PvPmo.Util
{
    public class SqlQry
    {
        public static Boolean NoQrySql(string nomeDb, string qry)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            Boolean Bol = SqlDb.SqlNoQry(qry, StrConn);
            return Bol;
        }
        public static async Task<bool> RunBulk(string nomeDb, string tabMod, DataTable tabBulk)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            //string Tab = tabella;
            Boolean Bol = await SqlDb.SqlBulkCopy(tabMod, StrConn, tabBulk);
            return Bol;
        }
        public static Boolean DelTabSql(string nomeDb, string tabella)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            string qry = "DROP TABLE IF EXISTS " + tabella + ";";
            Boolean Bol = SqlDb.SqlNoQry(qry, StrConn);
            return Bol;
        }
        public static Boolean DelRecSql(string nomeDb, string tabella)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            string qry = "DELETE FROM " + tabella + ";";
            Boolean Bol = SqlDb.SqlNoQry(qry, StrConn);
            return Bol;
        }
        public static Boolean AddColSql(string nomeDb, string tabella, string nuovaCol)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            string qry = "ALTER TABLE `" + tabella + "` ADD COLUMN " + nuovaCol + ";";
            Boolean Bol = SqlDb.SqlNoQry(qry, StrConn);
            return Bol;
        }
        public static async Task<DataTable> UpdTabSql(string nomeDb, string tabQry, DataTable tabella)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            string qry = "UPDATE `" + tabQry + "` SET *;";
            await SqlDb.SqlQry(StrConn, qry, tabella);
            return tabella;       }
        public static async Task<DataTable> NomiColSql(string nomeDb, string tabQry, DataTable tabella)
        {
            string StrConn = Conn.MysqlConn(nomeDb);
            string qry = "SHOW COLUMNS FROM `" + tabQry + "`; ";
            await SqlDb.SqlQry(StrConn, qry, tabella);
            return tabella;
        }
        //public static int EseguiQrySql(string nomeDb, string qry)
        //{
        //    string StrConn = Conn.MysqlConn(nomeDb);
        //    int Num = ConnDb.ConnSql.EseguiQry(qry, StrConn);
        //    return Num;
        //}
        //public static int EseguiQrySql(string nomeDb, string qry, DataTable tabImp)
        //{
        //    string StrConn = Conn.MysqlConn(nomeDb);
        //    int Num = ConnDb.ConnSql.EseguiQry(qry, StrConn, tabImp);
        //    return Num;
        //}
        //public static int EseguiQrySqlParam(string nomeDb, string qry, DataTable tabImp)
        //{
        //    string StrConn = Conn.MysqlConn(nomeDb);
        //    int Num = ConnDb.ConnSql.EseguiQryParam(qry, StrConn, tabImp);
        //    return Num;
        //}
    }
}
