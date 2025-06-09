namespace PvPmo.GestDb
{    
    // Questa classe gestisce il ciclo di vita delle connessioni al database.
    // Implementa IAsyncDisposable per garantire che tutte le connessioni vengano
    // chiuse correttamente quando l'oggetto non serve più.
    public partial class GestoreConnessioni : IAsyncDisposable
    {
        // Un dizionario per memorizzare le connessioni attive.
        // La chiave (string) è il nome che diamo alla connessione.
        // Il valore (MySqlConnection) è l'oggetto connessione vero e proprio.
        private readonly Dictionary<string, MySqlConnection> _connessioni = new();

        /// <summary>
        /// Crea una nuova connessione, la apre e la memorizza con un nome specifico.
        /// </summary>
        /// <param name="nomeConnessione">Il nome univoco da assegnare a questa connessione.</param>
        /// <param name="stringaConnessione">La stringa di connessione per il database.</param>
        /// <returns>L'oggetto MySqlConnection aperto e pronto all'uso.</returns>
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

        /// <summary>
        /// Recupera una connessione già aperta tramite il suo nome.
        /// </summary>
        /// <param name="nomeConnessione">Il nome della connessione da recuperare.</param>
        /// <returns>L'oggetto MySqlConnection se trovato, altrimenti null.</returns>
        public MySqlConnection? OttieniConnessione(string nomeConnessione)
        {
            _connessioni.TryGetValue(nomeConnessione, out var connection);
            return connection;
        }

        /// <summary>
        /// Chiude e rimuove una connessione specifica.
        /// </summary>
        /// <param name="nomeConnessione">Il nome della connessione da chiudere.</param>
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

        /// <summary>
        /// Metodo speciale di IAsyncDisposable. Viene chiamato automaticamente
        /// quando si usa "await using" per chiudere tutte le connessioni rimanenti.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("Il GestoreConnessioni sta eseguendo la pulizia...");
            foreach (var nome in _connessioni.Keys.ToList()) // Usiamo ToList() per creare una copia della lista delle chiavi
            {
                await ChiudiConnessioneAsync(nome);
            }
        }
    }
}
