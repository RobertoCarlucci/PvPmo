namespace PvPmo.GestDb
{
    public class SqlQry
    {
        public static async Task<bool> CreaIdMonthYear(string conn, string tab, string col, string concat) =>
            await UpdtColConcat(conn, tab, col, concat);

        public static async Task<bool> CreaKeyId(string conn, string tab, string col, string concat) =>
            await UpdtColConcat(conn, tab, col, concat);

        public static async Task<bool> CreaDateId(string conn, string tab, string col, string concat) =>
            await UpdtColConcat(conn, tab, col, concat);

        private static async Task<bool> UpdtColConcat(string conn, string tab, string col, string concat)
        {
            string sql = $"UPDATE `{tab}` SET `{col}` = CONCAT({concat});";
            return await SqlAsync.SqlNoQry(conn, sql, 180);
        }

        public static async Task<bool> AddColSql(string conn, string tab, string col, string type)
        {
            string sql = $"ALTER TABLE `{tab}` ADD COLUMN `{col}` {type};";
            return await SqlAsync.SqlNoQry(conn, sql, 30);
        }

        public static async Task<bool> DelColSql(string conn, string tab, string col)
        {
            string sql = $"ALTER TABLE `{tab}` DROP COLUMN IF EXISTS `{col}`;";
            return await SqlAsync.SqlNoQry(conn, sql, 30);
        }

        public static async Task<bool> DelRecSql(string db, string tab)
        {
            string conn = Conn.MysqlConn(db);
            string sql = $"DELETE FROM `{tab}`;";
            return await SqlAsync.SqlNoQry(conn, sql, 30);
        }

        public static async Task<bool> RinColSql(string conn, string tab, string oldCol, string newCol, string type)
        {
            string sql = $"ALTER TABLE `{tab}` CHANGE `{oldCol}` `{newCol}` {type};";
            return await SqlAsync.SqlNoQry(conn, sql, 30);
        }

        public static async Task<DataTable> NomiColSql(string conn, string tab, DataTable schema)
        {
            string qry = $"SHOW COLUMNS FROM `{tab}`;";
            await SqlAsync.SqlQryDataTable(conn, qry, schema);
            return schema;
        }
    }
    public class ExlQry
    {
        public static async Task<int> SelExlQry(string fileImp, string foglio)
        {
            string StrConn = Conn.ExlFileConn(fileImp);
            string qry = $"SELECT * FROM [{foglio}$];";
            int Num = await ExlAsync.ExcQry(qry, StrConn);
            return Num;
        }
        public static async Task<bool> SelExlQry(string fileImp, string foglio, DataTable tabImp)
        {
            string StrConn = Conn.ExlFileConn(fileImp);
            string qry = $"SELECT * FROM [{foglio}$];";
            bool Bol = await ExlAsync.ExcQry(qry, StrConn, tabImp);
            return Bol;
        }
    }
}
