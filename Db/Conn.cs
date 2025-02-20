namespace PvPmo.Db
{
    public class Conn
    {
        public Conn() { }

        public static string MysqlConn(string nomeDb) 
        {
            string NomeHost = "server=127.0.0.1;port=3306;";
            string UserName = "user=root;Pwd=root;database=";            
            string StrConn = NomeHost + UserName + nomeDb + ";";
            return StrConn;
        }
    }
}
