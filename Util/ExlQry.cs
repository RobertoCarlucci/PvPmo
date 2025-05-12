using System.Threading.Tasks;

namespace PvPmo.Util
{
    public class ExlQry
    {
        public static async Task<int> SelExlQry(string fileImp, string foglio)
        {
            string StrConn = Conn.ExlFileConn(fileImp);
            string qry = "SELECT * FROM [" + foglio + "$];";
            int Num = await ExlAsync.ExcQry(qry, StrConn);
            return Num;
        }
        public static async Task<bool> SelExlQry(string fileImp, string foglio, DataTable tabImp)
        {
            string StrConn = Conn.ExlFileConn(fileImp);
            string qry = "SELECT * FROM [" + foglio + "$];";
            bool Bol = await ExlAsync.ExcQry(qry, StrConn, tabImp);
            return Bol;
        }
    }
}
