namespace PvPmo.Model
{
    public class TabData
    {
        public DateTime? PV_TotalData { get; set; }
        public DateTime? GlobalTimesheetExtractData { get; set; }
        public DateTime? PV_MeseSuAnno { get; set; }

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

            string StrConn = Conn.MysqlConn("pmo");
            string Qry = "SELECT * From 01_tabella_data;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _inpdata.Add(new TabData()
                {
                    PV_TotalData = (DateTime)v["PV_TotalData"],
                    GlobalTimesheetExtractData = (DateTime)v["GlobalTimesheetExtractData"],
                    PV_MeseSuAnno = (DateTime)v["PV_MeseSuAnno"]                    
                });
            }
        }
        public IList<TabData> TabDatas => _inpdata;
    }
}
