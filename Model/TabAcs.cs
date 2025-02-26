namespace PvPmo.Model
{
    public class TabAcs
    {
        public required string db_inp { get; set; }
        public required string tabella { get; set; }
        public required string db_dest { get; set; }
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

            string StrConn = Db.Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine_acs;";
            Db.SqlDb.SqlQrySyn(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _inpacs.Add(new TabAcs()
                {
                    db_inp = v["db_inp"].ToString(),
                    tabella = v["tabella"].ToString(),
                    db_dest = v["db_dest"].ToString()
                });
            }
        }
        public IList<TabAcs> InpAcs => _inpacs;
    }
}
