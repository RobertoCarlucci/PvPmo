namespace PvPmo.ImportDb
{
    public partial class AcsToSql
    {
        public static async Task NewDb()
        {
            string _acsPath = await SelCart.PickFolderStatic(default);
            TabAcsService _inpacs = new TabAcsService();
            int x = 1;

            foreach (var n in _inpacs.InpAcs)
            {                
                string nomeDbAcs = n.db_inp;
                string nomeTbAcs = n.tabella;
                string nomeDbSql = n.db_dest;
                string tab = await TabAcstoTabSql(nomeDbAcs, nomeTbAcs, nomeDbSql, _acsPath);                
                string StrConnSql = Conn.MysqlConn("pvpmo_origine");
                string Qry = "UPDATE  origine_acs set tabella_sql = '" + tab + "' WHERE id = " + x + ";";
                SqlSync.SqlQry(StrConnSql, Qry);
                x++;
            }

            TabAcsService _inpacsdati = new TabAcsService();

            foreach (var n in _inpacsdati.InpAcs)
            {
                string nomeDbAcs = n.db_inp;
                string nomeTbAcs = n.tabella;
                string nomeDbSql = n.db_dest;
                string nomeTbSql = n.tabella_sql;
                await InpAcsDatitoSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, _acsPath);            
            }
        }
        public static async Task<string> TabAcstoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string acsPath)
        {
            string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0";
            await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
            string nomeTbNorm = VarUtil.NormNomeTab(nomeTbAcs);
            Qry = VarUtil.NormInp(nomeTbAcs, nomeTbNorm, _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;            
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            bool Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);            
            return nomeTbNorm;
        }
        public static async Task<bool> InpAcsDatitoSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string nomeTbSql, string acsPath)
        {
            bool bol = await VarUtil.CreaMappingAcsSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, acsPath);
            if(bol == true)
            {
                string StrConnAcs = Conn.AcsDbConn(nomeDbAcs, acsPath);
                DataTable _tabella = new DataTable();
                string Qry = "SELECT * FROM [" + nomeTbAcs + "]";
                await AcsAsync.AcsQryTab(StrConnAcs, Qry, _tabella);
                string StrConnSql = Conn.MysqlConn(nomeDbSql);
                await SqlAsync.SqlBulkCopy(StrConnSql, nomeTbSql, _tabella);
            }
            return true;            
        }
    }
}
