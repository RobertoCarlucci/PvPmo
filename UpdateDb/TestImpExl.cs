namespace PvPmo.UpdateDb
{
    public partial class TestImpExl
    {
        public static async Task<bool> TestFileExlImp()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("pvtotal",  typeof(string));
            dataTable.Columns.Add("gte", typeof(string));

            bool Bol = false;

            string StrConnSql = Conn.MysqlConn("pvpmo_origine");

            string QrySqlGte = "SELECT DISTINCT dateid FROM global_timesheet_extract ORDER BY dateid ASC;";
            string QrySqlPvt = "SELECT DISTINCT dateid FROM pv_total ORDER BY dateid ASC;";

            

            dataTable = await SqlAsync.SqlQryDataTable(StrConnSql, QrySqlGte, dataTable);
            TabDataService _tabDataService = new TabDataService();
            
            foreach (var n in _tabDataService.TabDatas)
            {
                dataTable = n.PV_TotalData.ToString();
                string? _globtimextr = n.GlobalTimesheetExtractData.ToString();
            } 
            return true;
        }
    }
}
