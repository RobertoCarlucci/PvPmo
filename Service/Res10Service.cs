using PvPmo.Db;

namespace PvPmo.Service
{
    public class Res10Service
    {
        public static async Task ListaRes10()
        {
            string nomeDb = "timesheet";
            string StrConn = Conn.MysqlConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * From res10;";
            await Database.QuerySemplice(StrConn, Qry, _tabella);           
                     
        }
    }
}
