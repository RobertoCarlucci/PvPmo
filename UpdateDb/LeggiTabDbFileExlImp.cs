namespace PvPmo.UpdateDb
{
    public class LeggiTabDbFileExlImp
    {        
        public string? tabella { get; set; }
        public string? WorkSheet { get; set; }
        public string? db_dest { get; set; }
        public string? tabella_sql { get; set; }        
    }
    public interface ILeggiTabDbFileExlImp
    {
        IList<LeggiTabDbFileExlImp> LeggiTabDbFileExlImp { get; }
    }
    public class LeggiTabDbFileExlImpService : ILeggiTabDbFileExlImp
    {
        private List<LeggiTabDbFileExlImp> _leggitabdbfileexlimp;

        public LeggiTabDbFileExlImpService() 
        {

            DataTable _elencoInp = new DataTable();
            _leggitabdbfileexlimp = new List<LeggiTabDbFileExlImp>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine_update;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _leggitabdbfileexlimp.Add(new LeggiTabDbFileExlImp()
                {
                    tabella = v["tabella"].ToString(),
                    WorkSheet = v["WorkSheet"].ToString(),
                    db_dest = v["db_dest"].ToString(),
                    tabella_sql = v["tabella_sql"].ToString()
                });
            }

        }
        public IList<LeggiTabDbFileExlImp> LeggiTabDbFileExlImp => _leggitabdbfileexlimp;
    }    
}
