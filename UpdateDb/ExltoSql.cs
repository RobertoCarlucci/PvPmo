namespace PvPmo.UpdateDb
{
    public partial class ExltoSql
    {
        // Path x caricare file dall'archivio viene aggiunto al nome file.
        private static string _cartInput = @"\Archivio\IN\Timesheet\";

        public static async Task InpExl()
        {
            ImpExlService _inpexl = new ImpExlService();
            int x = 1;

            foreach (var n in _inpexl.FileExl)
            {
                string nomeDbAcs = n.db_inp;
                string nomeTbAcs = n.tabella;
                string nomeDbSql = n.db_dest;
                string tab = await FileExltoTabSql(nomeDbAcs, nomeTbAcs, nomeDbSql, _cartInput);
                string StrConn = Conn.MysqlConn("pvpmo_origine");
                string Qry = "UPDATE  origine_acs set tabella_sql = '" + tab + "' WHERE id = " + x + ";";
                SqlSync.SqlQry(StrConn, Qry);
                x++;
            }

            //TabAcsService _inpacsdati = new TabAcsService();

            //foreach (var n in _inpacsdati.InpAcs)
            //{
            //    string nomeDbAcs = n.db_inp;
            //    string nomeTbAcs = n.tabella;
            //    string nomeDbSql = n.db_dest;
            //    string nomeTbSql = n.tabella_sql;
            //    await InpAcsDatitoSql(nomeDbAcs, nomeTbAcs, nomeDbSql, nomeTbSql, _acsPath);
            //}
        }
            public static async Task<string> FileExltoTabSql(string nomeDbAcs, string nomeTbAcs, string nomeDbSql, string acsPath)
        {
            string StrConnExl = Conn.ExFlConn( acsPath);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTbAcs + "] WHERE 1=0";
            await AcsAsync.AcsQryTab(StrConnExl, Qry, _tabella);
            string nomeTbNorm = VarUtil.NormNomeTab(nomeTbAcs);
            Qry = VarUtil.NormInp(nomeTbAcs, nomeTbNorm, _tabella);
            Qry = "CREATE OR REPLACE TABLE " + Qry;
            string StrConnSql = Conn.MysqlConn(nomeDbSql);
            bool Bol = await SqlAsync.SqlNoQry(StrConnSql, Qry);
            return nomeTbNorm;
        }
    } 
}

