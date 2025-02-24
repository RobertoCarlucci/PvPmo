using PvPmo.Db;

namespace PvPmo.Service
{
    public class AcsToSql
    {
        public static async Task CopiaAcstoSql()
        {
            string nomeDb = "PvPMO_Table";
            string StrConn = Conn.AcsDbConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [PV_Total] WHERE 1=0";
            await AcsDb.AcsQryTab(Qry, StrConn, _tabella);
            Qry = VarUtil.CreaTabDb("PV_Total", _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;
            nomeDb = "timesheet";
            StrConn = Conn.MysqlConn(nomeDb);
            bool Bol = SqlDb.SqlNoQry(Qry, StrConn);

            //nomeDb = "timesheet";
            //StrConn = Conn.MysqlConn(nomeDb);
            //await SqlDb.SqlBulkCopy("GlobalTimesheetExtract", StrConn, _tabella);
        }
    }
}
