using PvPmo.Db;

namespace PvPmo.Service
{
    public class AcsToSql
    {
        public static async Task NewDb()
        {
            TabAcsService service = new TabAcsService();

            foreach (var n in service.InpAcs)
            {
                string nomeDb = n.db_inp;
                string nomeTb = n.tabella;
                string destinazione = n.db_dest;
                await CopiaAcstoSql(nomeDb, nomeTb, destinazione);
            }
        }
        public static async Task CopiaAcstoSql(string nomeDb, string nomeTb, string destinazione)
        {            
            string StrConn = Conn.AcsDbConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTb + "] WHERE 1=0";
            await AcsDb.AcsQryTab(Qry, StrConn, _tabella);
            Qry = VarUtil.NormInp(nomeTb, _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;            
            StrConn = Conn.MysqlConn(destinazione);
            bool Bol = SqlDb.SqlNoQry(Qry, StrConn);            
        }
    }
}
