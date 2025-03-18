namespace PvPmo.Model
{
    public class NormImp
    {
        public string? colonna { get; set; }
        public string? azione { get; set; }        
        public string? tabella { get; set; }
    }
    public interface INormImp
    {
        IList<NormImp> NormImp { get; }
    }

    public class NormImpService : INormImp
    {
        private List<NormImp> _normimp;

        public NormImpService()
        {
            DataTable _elencoInp = new DataTable();
            _normimp = new List<NormImp>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From normalizza_importazioni;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _normimp.Add(new NormImp()
                {
                    colonna = v["Colonna"].ToString(),
                    azione = v["azione"].ToString(),
                    tabella = v["tabella"].ToString()                    
                });
            }
        }
        public IList<NormImp> NormImp => _normimp;
    }
}
