namespace PvPmo.GestDb
{    
    // Questa classe gestisce il ciclo di vita delle connessioni al database.
    // Implementa IAsyncDisposable per garantire che tutte le connessioni vengano
    // chiuse correttamente quando l'oggetto non serve più.
    public partial class GestConn : IAsyncDisposable
    {
        // Un dizionario per memorizzare le connessioni attive.
        // La chiave (string) è il nome che diamo alla connessione.
        // Il valore (MySqlConnection) è l'oggetto connessione vero e proprio.
        private readonly Dictionary<string, MySqlConnection> _connessioni = new();
                
        public async Task<MySqlConnection> CreaEApriConnessioneAsync(string nomeConnessione, string stringaConnessione)
        {
            // Controlliamo se una connessione con questo nome esiste già, per evitare errori.
            if (_connessioni.ContainsKey(nomeConnessione))
            {
                throw new InvalidOperationException($"Una connessione con il nome '{nomeConnessione}' esiste già.");
            }

            Console.WriteLine($"Creo e apro la connessione '{nomeConnessione}'...");
            var connection = new MySqlConnection(stringaConnessione);
            await connection.OpenAsync();

            // Aggiungiamo la connessione appena aperta al nostro dizionario.
            _connessioni[nomeConnessione] = connection;

            Console.WriteLine($"Connessione '{nomeConnessione}' aperta e pronta.");
            return connection;
        }        
        public MySqlConnection? OttieniConnessione(string nomeConnessione)
        {
            _connessioni.TryGetValue(nomeConnessione, out var connection);
            return connection;
        }
        public async Task ChiudiConnessioneAsync(string nomeConnessione)
        {
            if (_connessioni.TryGetValue(nomeConnessione, out var connection))
            {
                Console.WriteLine($"Chiusura della connessione '{nomeConnessione}'...");
                await connection.CloseAsync();
                _connessioni.Remove(nomeConnessione);
                Console.WriteLine($"Connessione '{nomeConnessione}' chiusa.");
            }
        }
        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("Il GestoreConnessioni sta eseguendo la pulizia...");
            foreach (var nome in _connessioni.Keys.ToList()) // Usiamo ToList() per creare una copia della lista delle chiavi
            {
                await ChiudiConnessioneAsync(nome);
            }
        }
    }
    public static class Conn
    {
        public static string MysqlConn(string nomeDb)
        {
            string NomeHost = "server = 127.0.0.1; port=3306;";
            string UserName = "user = root; Pwd = root;database = ";
            string StrConn = NomeHost + UserName + nomeDb + ";";
            return StrConn;
        }
        public static string AcsDbConn(string nomeDb, string nomePath)
        {
            string StrConn = "Provider = Microsoft.ACE.OLEDB.16.0; Data Source = "
                + nomePath + "\\" + nomeDb + ".accdb;";
            return StrConn;
        }
        public static string ExlFileConn(string fileImp)
        {
            string StrConn = "Provider = Microsoft.ACE.OLEDB.16.0; Data Source = "
                + fileImp + ".xlsx; Extended Properties = Excel 12.0 Xml;";
            return StrConn;
        }
    }
}
