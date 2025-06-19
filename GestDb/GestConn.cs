namespace PvPmo.GestDb
{    
    public static class Conn
    {
        public static async Task<string> MysqlConn(string nomeDb)
        {
            string StrConn = string.Empty;
            bool connessioneOk = true;

            if (!string.IsNullOrWhiteSpace(nomeDb))
            {
                string NomeHost = "server = 127.0.0.1; port = 3306;";
                string UserName = "user = root; Pwd = root; database = ";
                StrConn = NomeHost + UserName + nomeDb + ";";
            }
            connessioneOk = await SqlAsync.TestConnSql(StrConn);
            StrConn = connessioneOk ? StrConn : string.Empty;
            return StrConn;
        }
        public static string MysqlConnSer (string nomeDb)
        {
            string StrConn = string.Empty;
            //bool connessioneOk = true;

            if (!string.IsNullOrWhiteSpace(nomeDb))
            {
                string NomeHost = "server = 127.0.0.1; port = 3306;";
                string UserName = "user = root; Pwd = root; database = ";
                StrConn = NomeHost + UserName + nomeDb + ";";
            }
            //connessioneOk = await SqlAsync.TestConnSql(StrConn);
            //StrConn = connessioneOk ? StrConn : string.Empty;
            return StrConn;
        }
        public static async Task<string> AcsDbConn(string nomeDb, string nomePath)
        {
            string StrConn = string.Empty;
            bool connessioneOk = true;

            if (!string.IsNullOrWhiteSpace(nomeDb) || !string.IsNullOrWhiteSpace(nomePath))
            {
                StrConn = "Provider = Microsoft.ACE.OLEDB.16.0; Data Source = "
                + nomePath + "\\" + nomeDb + ".accdb;";
            }
            connessioneOk = await AcsAsync.TestConnAcs(StrConn);
            StrConn = connessioneOk ? StrConn : string.Empty;
            return StrConn;           
        }
        public static async Task<string> ExlFileConn(string fileImp)
        {
            string StrConn = string.Empty;
            bool connessioneOk = true;

            if (!string.IsNullOrWhiteSpace(fileImp))
            {
               StrConn = "Provider = Microsoft.ACE.OLEDB.16.0; Data Source = "
               + fileImp + ".xlsx; Extended Properties = Excel 12.0 Xml;";
            }
            connessioneOk = await ExlAsync.TestConnExl(StrConn);
            StrConn = connessioneOk ? StrConn : string.Empty;            
            return StrConn;
        }
    }
}
