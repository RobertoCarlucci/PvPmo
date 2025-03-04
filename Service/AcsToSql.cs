using PvPmo.Db;

namespace PvPmo.Service
{
    public class AcsToSql
    {
        public static async Task NewDb()
        {
            TabAcsService _inpacs = new TabAcsService();
            int x = 1;

            foreach (var n in _inpacs.InpAcs)
            {                
                string nomeDbAcs = n.db_inp;
                string nomeTbAcs = n.tabella;
                string nomeDbSql = n.db_dest;
                string tab = await TabAcstoTabSql(nomeDbAcs, nomeTbAcs, nomeDbSql);                
                string StrConn = Conn.MysqlConn("pvpmo_origine");
                string Qry = "UPDATE  origine_acs set tabella_sql = '" + tab + "' WHERE id = " + x + ";";
                SqlSync.SqlQry(StrConn, Qry);
                x++;
            }

            TabAcsService _inpacsdati = new TabAcsService();

            foreach (var n in _inpacsdati.InpAcs)
            {
                string nomeDbAcs = n.db_inp;
                string nomeTbAcs = n.tabella;
                string nomeDbSql = n.db_dest;
                string nomeTbSql = n.tabella_sql;
                await InpAcsDatitoSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql);            
            }
        }
        public static async Task<string> TabAcstoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql)
        {
            string StrConn = Conn.AcsDbConn(nomeDbAcs);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0";
            await AcsAsync.AcsQryTab(StrConn, Qry, _tabella);
            string nomeTbNorm = VarUtil.NormNomeTab(nomeTbAcs);
            Qry = VarUtil.NormInp(nomeTbAcs, nomeTbNorm, _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;            
            StrConn = Conn.MysqlConn(nomeDbSql);
            bool Bol = await SqlAsync.SqlNoQry(StrConn, Qry);            
            return nomeTbNorm;
        }
        public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql)
        {
            bool bol = await VarUtil.CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql);
            if(bol == true)
            {
                string StrConn = Conn.AcsDbConn(nomeDbAcs);
                DataTable _tabella = new DataTable();
                string Qry = "SELECT * FROM [" + nomeTbAcs + "]";
                await AcsAsync.AcsQryTab(StrConn, Qry, _tabella);
                StrConn = Conn.MysqlConn(nomeDbSql);
                await SqlAsync.SqlBulkCopy(StrConn, nomeTbSql, _tabella);
            }
            return true;            
        }
    }
}
