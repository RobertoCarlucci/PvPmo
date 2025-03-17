namespace PvPmo.UpdateDb
{
    public partial class ExltoSql
    {
        public static async Task InpExl()
        {
            string _exlPath = await SelCart.PickFolderStatic(default);
            ImpExlService _inpexl = new ImpExlService();            

            foreach (var n in _inpexl.FileExl)
            {
                DataTable _tabella = new DataTable();
                string nomeFoglio = n.WorkSheet;
                string nomeTbExl = n.tabella;
                string nomeDbSql = n.db_dest;
                string nomeTdDest = n.tabella_sql;
                string StrConnExl = Conn.ExlFileConn(_exlPath + "\\" + nomeTbExl);
                string QryExl = "SELECT * FROM [" + nomeFoglio + "$] WHERE 1=0;";
                Boolean Bol = ExlSync.ExcQry(StrConnExl, QryExl, _tabella);
                string QrySql = VarUtil.InpExl(nomeTdDest, _tabella);
                string StrConnSql = Conn.MysqlConn(nomeDbSql);
                QrySql = "CREATE OR REPLACE TABLE " + QrySql;
                Bol = await SqlAsync.SqlNoQry(StrConnSql, QrySql);
                await SqlAsync.SqlBulkCopy(StrConnSql, nomeTdDest, _tabella);
                //string StrConnSql = Conn.MysqlConn("pmo");
                //string QrySql = "SHOW COLUMNS FROM " + nomeTdDest;
                //DataTable _showcolumn = new DataTable();
                //await SqlAsync.SqlQryDataTable(StrConnSql, QrySql, _showcolumn);



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
            string StrConnExl = Conn.ExlFileConn( acsPath);
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

