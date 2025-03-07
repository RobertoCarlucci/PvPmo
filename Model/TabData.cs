namespace PvPmo.Model
{
    public class TabData
    {
        //public int id { get; }
        public DateOnly PV_TotalData { get; set; }
        public DateOnly GlobalTimesheetExtractData { get; set; }
        public DateOnly PV_MeseSuAnno { get; set; }

    }
    public interface ITabDataService
    {
        IList<TabData> TabDatas { get; }
    }
    public class TabDataService : ITabDataService
    {
        private List<TabData> _inpdata;

        public TabDataService()
        {
            DataTable _elencoInp = new DataTable();
            _inpdata = new List<TabData>();

            string StrConn = Db.Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine_acs;";
            Db.SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _inpdata.Add(new TabData()
                {
                    PV_TotalData = (DateOnly)v["PV_TotalData"],
                    GlobalTimesheetExtractData = (DateOnly)v["GlobalTimesheetExtractData"],
                    PV_MeseSuAnno = (DateOnly)v["PV_MeseSuAnno"]                    
                });
            }
        }
        public IList<TabData> TabDatas => _inpdata;
    }
}
