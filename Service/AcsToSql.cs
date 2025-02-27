using Microsoft.Maui.Controls;
using PvPmo.Db;

namespace PvPmo.Service
{
    public class AcsToSql
    {
        public static async Task NewDb()
        {
            TabAcsService _inpacs = new TabAcsService();
            //DataTable _exp = new DataTable();
            //DataColumn _column = _exp.Columns.Add("db_inp", typeof(string));
            //_exp.Columns.Add("tabella", typeof(string));
            //_exp.Columns.Add("db_dest", typeof(string));
            //_exp.Columns.Add("tabella_sql", typeof(string));

            foreach (var n in _inpacs.InpAcs)
            {
                int x = 1;
                string nomeDb = n.db_inp;
                string nomeTb = n.tabella;
                string destinazione = n.db_dest;
                string tab = await CopiaAcstoSql(nomeDb, nomeTb, destinazione);
                //n.tabella_sql = tab;
                string StrConn = Conn.MysqlConn("pvpmo_origine");
                string Qry = "UPDATE  origine_acs set tabella_sql " + tab + " WHERE id = " + x + ";";
                SqlDb.SqlQrySyn(StrConn, Qry);
                x++;
            }
            
            
            
        }
        public static async Task<string> CopiaAcstoSql(string nomeDb, string nomeTb, string destinazione)
        {
            //Res10Service _inpres10 = new Res10Service();

            string StrConn = Conn.AcsDbConn(nomeDb);
            DataTable _tabella = new DataTable();
            string Qry = "SELECT * FROM [" + nomeTb + "] WHERE 1=0";
            await AcsDb.AcsQryTab(Qry, StrConn, _tabella);
            string nomeTbNorm = VarUtil.NormNomeTab(nomeTb);
            Qry = VarUtil.NormInp(nomeTb, nomeTbNorm, _tabella);
            Qry = "CREATE OR REPLACE TABLE " +  Qry;            
            StrConn = Conn.MysqlConn(destinazione);
            bool Bol = SqlDb.SqlNoQry(Qry, StrConn);
            //string tab = await InpAcstoSql(nomeDb, nomeTbNorm, nomeTb, destinazione);
            return nomeTbNorm;
        }
        public static async Task InpAcstoSql(string nomeDb, string nomeTb, string nomeTbNorm, string destinazione)
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
