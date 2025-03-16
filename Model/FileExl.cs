namespace PvPmo.Model
{
    public class FileExl
    {        
        public string? db_inp { get; set; }
        public string? tabella { get; set; }
        public string? db_dest { get; set; }
        public string? tabella_sql { get; set; }        
    }
    public interface IFileExl
    {
        IList<FileExl> FileExl { get; }
    }
    public class ImpExlService : IFileExl
    {
        private List<FileExl> _inpuexl;

        public ImpExlService() 
        {

            DataTable _elencoInp = new DataTable();
            _inpuexl = new List<FileExl>();

            string StrConn = Conn.MysqlConn("pvpmo_origine");
            string Qry = "SELECT * From origine_update;";
            SqlSync.SqlQryDataTable(StrConn, Qry, _elencoInp);
            DataRow[] temp = _elencoInp.Select();
            foreach (DataRow v in temp)
            {
                _inpuexl.Add(new FileExl()
                {
                    db_inp = v["db_inp"].ToString(),
                    tabella = v["tabella"].ToString(),
                    db_dest = v["db_dest"].ToString(),
                    tabella_sql = v["tabella_sql"].ToString()
                });
            }

        }
        public IList<FileExl> FileExl => _inpuexl;
    }    
}
