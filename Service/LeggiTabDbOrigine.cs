namespace PvPmo.Service
{
    public class LeggiTabDbOrigine
    {
        public string? DbInp { get; set; }
        public string? Tabella { get; set; }
        public string? DbDest { get; set; }
        public string? TabellaSql { get; set; }
        public string? WorkSheet { get; set; }
        public string? InpType { get; set; }
    }
    public interface ILeggiTabDbOrigine
    {
        IList<LeggiTabDbOrigine> LeggiTabDbOrigine { get; }
    }

    public class LeggiTabDbOrigineService : ILeggiTabDbOrigine
    {
        private List<LeggiTabDbOrigine> _leggitabdborigine;

        public LeggiTabDbOrigineService()
        {
            DataTable _elencoInp = new DataTable();
            _leggitabdborigine = new List<LeggiTabDbOrigine>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _leggitabdborigine.Add(new LeggiTabDbOrigine()
                {
                    DbInp = v["DbInp"].ToString(),
                    Tabella = v["Tabella"].ToString(),
                    DbDest = v["DbDest"].ToString(),
                    TabellaSql = v["TabellaSql"].ToString(),
                    WorkSheet = v["WorkSheet"].ToString(),
                    InpType = v["InpType"].ToString()
                });
            }
        }
        public IList<LeggiTabDbOrigine> LeggiTabDbOrigine => _leggitabdborigine;
    }
}
