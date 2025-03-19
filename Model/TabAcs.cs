namespace PvPmo.Model
{
    public class TabAcs
    {
        public string? DbInp { get; set; }
        public string? Tabella { get; set; }
        public string? DbDest { get; set; }
        public string? TabellaSql { get; set; }
    }
    public interface ITabAcsService
    {
        IList<TabAcs> InpAcs { get; }
    }

    public class TabAcsService : ITabAcsService
    {
        private List<TabAcs> _inpacs;

        public TabAcsService()
        {
            DataTable _elencoInp = new DataTable();
            _inpacs = new List<TabAcs>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine_acs;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _inpacs.Add(new TabAcs()
                {
                    DbInp = v["DbInp"].ToString(),
                    Tabella = v["Tabella"].ToString(),
                    DbDest = v["DbDest"].ToString(),
                    TabellaSql = v["TabellaSql"].ToString()
                });
            }
        }
        public IList<TabAcs> InpAcs => _inpacs;
    }
}
