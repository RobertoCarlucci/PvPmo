namespace PvPmo.GestDb
{
    public static class Conn
    {
        public static string MysqlConn(string nomeDb) 
        {
            string NomeHost = "server=127.0.0.1;port=3306;";
            string UserName = "user=root;Pwd=root;database=";            
            string StrConn = NomeHost + UserName + nomeDb + ";";
            return StrConn;
        }
        public static string AcsDbConn(string nomeDb, string nomePath)
        {
            string StrConn = "Provider=Microsoft.ACE.OLEDB.16.0; Data Source=" + nomePath + "\\" + nomeDb + ".accdb;";
            return StrConn;
        }
        public static string ExFlConn(string fileImp)
        {
            string StrConn = "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=C:\\TestDb\\GesDb\\Aggiorna\\Timesheet\\"
                + fileImp + ".xlsx;Extended Properties=Excel 12.0 Xml;";
            return StrConn;
        }
    }
}
