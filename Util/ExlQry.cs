namespace PvPmo.Util
{
    public class ExlQry
    {
        public static int SelExlQry(string fileImp, string foglio)
        {
            string StrConn = Conn.ExlFileConn(fileImp);
            string qry = "SELECT * FROM [" + foglio + "$];";
            int Num = ExlSync.ExcQry(qry, StrConn);
            return Num;
        }
        public static Boolean SelExlQry(string fileImp, string foglio, DataTable tabImp)
        {
            string StrConn = Conn.ExlFileConn(fileImp);
            string qry = "SELECT * FROM [" + foglio + "$];";
            Boolean Bol = ExlSync.ExcQry(qry, StrConn, tabImp);
            return Bol;
        }
    }
}
