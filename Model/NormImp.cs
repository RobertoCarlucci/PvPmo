namespace PvPmo.Model
{
    public class NormImp
    {
        public string? Colonna { get; set; }
        public string? Azione { get; set; }        
        public string? Tabella { get; set; }
        public string? Dbdest { get; set; }

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
                    Colonna = v["colonna"].ToString(),
                    Azione = v["azione"].ToString(),
                    Tabella = v["tabella"].ToString(),
                    Dbdest = v["dbdest"].ToString()
                });
            }
        }
        public IList<NormImp> NormImp => _normimp;
    }
}
