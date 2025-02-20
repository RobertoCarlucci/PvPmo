using PvPmo.Db;

namespace PvPmo.Service
{
    public class AcsToSql
    {
        public static async Task CopiaAcstoSql()
        {
            string nomeDb = "PvPMO_Chart";
            string StrConn = Conn.AcsDbConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM GlobalTimesheetExtract;";
            await AcsDb.EseguiQry(Qry, StrConn, _tabella);
            nomeDb = "timesheet";
            StrConn = Conn.MysqlConn(nomeDb);
            await SqlDb.EseguiBulk("GlobalTimesheetExtract", StrConn, _tabella);
        }
    }
}
