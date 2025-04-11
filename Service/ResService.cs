using PvPmo.GestDb;

namespace PvPmo.Service
{
    public class ResService
    {
        public static async Task ListaRes10()
        {
            string nomeDb = "timesheet";
            string StrConn = Conn.MysqlConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * From res10;";
            await SqlAsync.SqlQryDataTable(StrConn, Qry, _tabella, 30);
            List<Res10> _list = VarUtil.ConvDTtoList<Res10>(_tabella);
        }        
    }
}
