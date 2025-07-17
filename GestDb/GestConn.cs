namespace PvPmo.GestDb
{
    public static class Conn
    {
        public static async Task<string> MysqlConn(string nomeDb)
        {
            if (string.IsNullOrWhiteSpace(nomeDb)) return string.Empty;

            string baseConn = AppConfig.GetConnectionString("MySqlDefault");
            string fullConn = $"{baseConn}{nomeDb};";            

            bool connessioneOk = await SqlAsync.TestConnSql(fullConn);
            return connessioneOk ? fullConn : string.Empty;
        }

        public static string MysqlConnSer(string nomeDb)
        {
            if (string.IsNullOrWhiteSpace(nomeDb)) return string.Empty;

            string baseConn = AppConfig.GetConnectionString("MySqlDefault");
            return $"{baseConn}database={nomeDb};";
        }

        public static async Task<string> AcsDbConn(string nomeDb, string nomePath)
        {
            if (string.IsNullOrWhiteSpace(nomeDb) || string.IsNullOrWhiteSpace(nomePath)) return string.Empty;

            string provider = AppConfig.GetConnectionString("OleDbProvider");
            string fullConn = $"{provider}Data Source={nomePath}\\{nomeDb}.accdb;";

            bool connessioneOk = await AcsAsync.TestConnAcs(fullConn);
            return connessioneOk ? fullConn : string.Empty;
        }

        public static async Task<string> ExlFileConn(string fileImp)
        {
            if (string.IsNullOrWhiteSpace(fileImp)) return string.Empty;

            string provider = AppConfig.GetConnectionString("OleDbProvider");
            string fullConn = $"{provider}Data Source={fileImp}.xlsx;Extended Properties=Excel 12.0 Xml;";

            bool connessioneOk = await ExlAsync.TestConnExl(fullConn);
            return connessioneOk ? fullConn : string.Empty;
        }
    }    
}
