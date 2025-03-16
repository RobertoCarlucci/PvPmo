namespace PvPmo.Model
{
    public class InpUpdt
    {        
        public string? db_inp { get; set; }
        public string? tabella { get; set; }
        public string? db_dest { get; set; }
        public string? tabella_sql { get; set; }        
    }
    public interface IInpUptd
    {
        IList<InpUpdt> InpUpdt { get; }
    }
    public class ImpUptdService : IInpUptd
    {
        private List<InpUpdt> _inpuptd;

        public ImpUptdService() 
        {

            DataTable _elencoInp = new DataTable();
            _inpuptd = new List<InpUpdt>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine_update;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _inpuptd.Add(new InpUpdt()
                {
                    db_inp = v["db_inp"].ToString(),
                    tabella = v["tabella"].ToString(),
                    db_dest = v["db_dest"].ToString(),
                    tabella_sql = v["tabella_sql"].ToString()
                });
            }

        }
        public IList<InpUpdt> InpUpdt => _inpuptd;
    }    
}
