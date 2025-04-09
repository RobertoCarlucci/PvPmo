namespace PvPmo.Service
{
    public class CaricaTabFinalizza
    {
        public string? Azione {  get; set; }
        public string? TabConfronto { get; set; }
        public string? ColConfronto { get; set; }
        public string? TabTestare { get; set; }
        public string? ColDaTestare { get; set; }
        public string? DbTabConfronto { get; set; }
        public string? DbTabTest { get; set; }

    }
    public interface ICaricaTabFinalizza
    {
        IList<CaricaTabFinalizza> caricaTabFin { get; }
    }
    public class CaricaTabFinalizzaService : ICaricaTabFinalizza
    {
        private List<CaricaTabFinalizza> _caricatabfinalizza;
        public CaricaTabFinalizzaService()
        {
            DataTable _elencoInp = new DataTable();
            _caricatabfinalizza = new List<CaricaTabFinalizza>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From finalizza;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _caricatabfinalizza.Add(new CaricaTabFinalizza()
                {
                    Azione = v["Azione"].ToString(),
                    TabConfronto = v["TabConfronto"].ToString(),
                    ColConfronto = v["ColConfronto"].ToString(),
                    TabTestare = v["TabTestare"].ToString(),
                    ColDaTestare = v["ColDaTestare"].ToString(),
                    DbTabConfronto = v["DbTabConfronto"].ToString(),
                    DbTabTest = v["DbTabTest"].ToString()
                });
            }
        }
        public IList<CaricaTabFinalizza> caricaTabFin => _caricatabfinalizza;
    }    
}
