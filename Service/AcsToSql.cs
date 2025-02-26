using PvPmo.Db;

namespace PvPmo.Service
{
    public class AcsToSql
    {
        public static async Task NewDb()
        {
            TabAcsService _inpacs = new TabAcsService();

            foreach (var n in _inpacs.InpAcs)
            {
                string nomeDb = n.db_inp;
                string nomeTb = n.tabella;
                string destinazione = n.db_dest;
                await CopiaAcstoSql(nomeDb, nomeTb, destinazione);
            }
        }
        public static async Task CopiaAcstoSql(string nomeDb, string nomeTb, string destinazione)
        {
            Res10Service _inpres10 = new Res10Service();

            string StrConn = Conn.AcsDbConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTb + "] WHERE 1=0";
            await AcsDb.AcsQryTab(Qry, StrConn, _tabella);
            Qry = VarUtil.NormInp(nomeTb, _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;            
            StrConn = Conn.MysqlConn(destinazione);
            bool Bol = SqlDb.SqlNoQry(Qry, StrConn);
            await InpAcstoSql(nomeDb, nomeTb, destinazione);
        }
        public static async Task InpAcstoSql(string nomeDb, string nomeTb, string destinazione)
        {
            ResService _inpres10 = new ResService();

            return;

            //foreach (var n in _inpres10.InpRes10)
            //{
            //    string nomeDb = n.db_inp;
            //    string nomeTb = n.tabella;
            //    string destinazione = n.db_dest;
            //    await CopiaAcstoSql(nomeDb, nomeTb, destinazione);
            //}
        }
    }
}
