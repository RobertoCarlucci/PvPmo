namespace PvPmo.Service
{
    public class CaricaTabOrigini
    {
        public string? DbInp { get; set; }
        public string? Tabella { get; set; }
        public string? DbDest { get; set; }
        public string? TabellaSql { get; set; }
        public string? WorkSheet { get; set; }
        public string? InpType { get; set; }
    }
    public interface ICaricaTabOrigini
    {
        IList<CaricaTabOrigini> CaricaTabOrigini { get; }
    }

    public class CaricaTabOriginiService : ICaricaTabOrigini
    {
        private List<CaricaTabOrigini> _caricataborigini;

        public CaricaTabOriginiService()
        {
            DataTable _elencoInp = new DataTable();
            _caricataborigini = new List<CaricaTabOrigini>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _caricataborigini.Add(new CaricaTabOrigini()
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
        public IList<CaricaTabOrigini> CaricaTabOrigini => _caricataborigini;
    }
}
