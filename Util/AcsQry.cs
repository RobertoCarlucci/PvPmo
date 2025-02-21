using PvPmo.Db;

namespace PvPmo.Util
{
    public class AcsQry
    {
        public static int SelExlQry(string fileImp, string foglio)
        {
            string StrConn = Conn.ExFlConn(fileImp);
            string qry = "SELECT * FROM [" + foglio + "$];";
            int Num = ExlFl.ExcQry(qry, StrConn);
            return Num;
        }
        public static Boolean SelExlQry(string fileImp, string foglio, DataTable tabImp)
        {
            string StrConn = Conn.ExFlConn(fileImp);
            string qry = "SELECT * FROM [" + foglio + "$];";
            Boolean Bol = ExlFl.ExcQry(qry, StrConn, tabImp);
            return Bol;
        }
    }
}
