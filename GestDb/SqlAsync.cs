namespace PvPmo.GestDb;

public partial class SqlAsync
{    
    //public static List<MySqlParameter> Params = new List<MySqlParameter>();
    //public static void AddParam(string nome, Object vale)
    //{
    //    var NewParam = new MySqlParameter(nome, vale);
    //    Params.Add(NewParam);
    //}
    
    public static async Task<bool> SqlNoQry(
        string strConn, string qry, int timeoutSec = 0, List<MySqlParameter>? parameters = null)
    {
        await using var conn = new MySqlConnection(strConn);
        try
        {            
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(qry, conn);
            cmd.CommandTimeout = timeoutSec;
            if (parameters != null)
                cmd.Parameters.AddRange(parameters.ToArray());
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (MySqlException ex)
        {
            await DbErrorHandler.ShowErrorAsync(ex, "Esecuzione SQL");
            return false;
        }
        finally
        {
            if (conn != null && conn.State == ConnectionState.Open) // Fixed condition
            {
                await conn.CloseAsync();
            }
        }
    }
    public static async Task<DataTable> SqlQryDataTable(
        string strConn, string qry, DataTable tabella, int timeoutSec = 0, List<MySqlParameter>? parameters = null)
    {
        await using var conn = new MySqlConnection(strConn);
        try
        {            
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(qry, conn);
            cmd.CommandTimeout = timeoutSec;
            if (parameters != null)
                cmd.Parameters.AddRange(parameters.ToArray());
            var _adapter = new MySqlDataAdapter(cmd);
            int _contarecord = _adapter.Fill(tabella);
            return tabella;
        }
        catch (MySqlException ex)
        {
            await DbErrorHandler.ShowErrorAsync(ex, "Esecuzione SQL");
            throw;
        }
        finally
        {
            if (conn != null && conn.State == ConnectionState.Open) // Fixed condition
            {
                await conn.CloseAsync();
            }
        }
    }

    public static async Task<bool> SqlBulkCopy(
        string strConn, string tableName, DataTable data, List<MySqlBulkCopyColumnMapping>? Mappings = null, int timeoutSec = 0)
    {
        strConn += "AllowLoadLocalInfile=true;";
        await using var conn = new MySqlConnection(strConn);
        try
        {
            await conn.OpenAsync();
            var bulk = new MySqlBulkCopy(conn)
            {
                BulkCopyTimeout = timeoutSec,
                DestinationTableName = tableName
            };
            if (Mappings != null)                 
                Mappings.ForEach(_mapping => { bulk.ColumnMappings.Add(_mapping); });
                Mappings.Clear();            
            
            await bulk.WriteToServerAsync(data);
            return true;
        }
        catch (MySqlException ex)
        {
            await DbErrorHandler.ShowErrorAsync(ex, "Esecuzione SQL");
            return false;
        }
        finally
        {
            if (conn != null && conn.State == ConnectionState.Open) // Fixed condition
            {
                await conn.CloseAsync();
            }
        }
    }
    
    // Inserisce o aggiorna (upsert) in modo massivo i dati da un DataTable in una tabella MariaDB di destinazione
    // utilizzando una tabella di staging temporanea.    
   
    public static async Task<bool> SqlBulkUpsertAsync(
        string strConn, string tableName, DataTable data, int timeoutSec = 0)
    {
        strConn += "AllowLoadLocalInfile=true;";
        //var connBulk = new MySqlConnection(strConn);

        if (data == null || data.Rows.Count == 0)
        {
            Console.WriteLine("DataTable è vuoto. Nessuna operazione di upsert eseguita.");
            return true; // Considerato successo perché non c'era nulla da fare.
        }
        if (data.Columns.Count == 0)
        {
            Console.WriteLine("DataTable non ha colonne.");
            return false;
        }

        // Usa backtick per i nomi, per sicurezza
        // (anche se non necessario in MySQL/MariaDB, è una buona pratica per evitare conflitti con parole chiave).

        string nomeTabDestinazione = $"`{tableName}`";
        //string nomeTabStaging = $"`staging_{tableName}_{Guid.NewGuid().ToString("N")}`"; // Nome univoco per la tabella di staging
        string nomeTabStaging = $"`staging_{tableName}`"; // Nome univoco per la tabella di staging
        try
        {
            // 1. Creare la tabella di staging temporanea (con struttura identica alla tabella di destinazione)
            //    CREATE TEMPORARY TABLE è l'ideale perché viene eliminata automaticamente alla fine della sessione.

            string creaStagingTabSql = $"CREATE TEMPORARY TABLE {nomeTabStaging} LIKE {nomeTabDestinazione};";
            if (!await SqlNoQry(strConn, creaStagingTabSql, timeoutSec))
            {
                Console.WriteLine($"Fallimento nella creazione della tabella di staging {nomeTabStaging}.");
                return false;
            }

            // 2. Eseguire il BulkCopy dei dati nella tabella di staging
            //    Questa sezione crea un'istanza locale di MySqlBulkCopy per gestire i mapping internamente.
           
            await using (var connBulk = new MySqlConnection(strConn))
            {
                await connBulk.OpenAsync();
                var bulkCopy = new MySqlBulkCopy(connBulk)
                {
                    DestinationTableName = nomeTabStaging.Trim('`'), // MySqlBulkCopy vuole il nome senza backtick
                    BulkCopyTimeout = timeoutSec
                };

                // Mapping delle colonne: da ordinale nel DataTable a nome colonna nella tabella di staging.
                // Questo presume che i nomi delle colonne nel DataTable corrispondano a quelli nella tabella di destinazione (e quindi di staging).

                foreach (DataColumn column in data.Columns)
                {
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(column.Ordinal, column.ColumnName));
                }
                await bulkCopy.WriteToServerAsync(data);
            }

            // 3. Costruire ed eseguire il comando INSERT ... ON DUPLICATE KEY UPDATE
            //    per eseguire l'upsert dalla tabella di staging alla tabella di destinazione.

            var dtNomeCol = data.Columns.Cast<DataColumn>().Select(c => $"`{c.ColumnName}`").ToList();
            string insListCol = string.Join(", ", dtNomeCol);

            // 4. Costruisci la parte SET per l'UPDATE. Aggiorna tutte le colonne fornite.
            // VALUES(nome_colonna) si riferisce al valore che sarebbe stato inserito.
            // Quindi, se la chiave primaria esiste già, aggiorna il valore con quello della tabella di staging.

            var updtListAssegnazioni = dtNomeCol.Select(colName => $"{colName} = VALUES({colName})").ToList();
            string updtAssegnazioni = string.Join(", ", updtListAssegnazioni);

            string qrySql = $"INSERT INTO {nomeTabDestinazione} ({insListCol}) " +
                              $"SELECT {insListCol} FROM {nomeTabStaging} " +
                              $"ON DUPLICATE KEY UPDATE {updtAssegnazioni};";

            if (!await SqlNoQry(strConn, qrySql, timeoutSec))
            {
                Console.WriteLine("Fallimento nell'operazione di merge/upsert dalla tabella di staging.");
                // La transazione implicita di SqlNoQry dovrebbe aver gestito il rollback se necessario a quel livello.
                return false;
            }

            Console.WriteLine($"Upsert massivo completato con successo per la tabella {tableName}.");
            return true;
        }
        catch (MySqlException ex)
        {
            await DbErrorHandler.ShowErrorAsync(ex, "Esecuzione SQL");
            throw;
        }
        catch (Exception ex)
        {
            // DbErrorHandler.ShowErrorAsync viene chiamato da SqlNoQry in caso di MySqlException.
            // Qui gestiamo altre eccezioni che potrebbero verificarsi in questo metodo.

            Console.WriteLine($"Errore durante SqlBulkUpsertAsync per la tabella {tableName}: {ex.Message}");            
            return false;
        }
        finally
        {
            // 4. Eliminare la tabella di staging temporanea, avendousato "CREATE TEMPORARY TABLE", questa viene eliminata
            // automaticamente alla chiusura della connessione/sessione. Tuttavia, è buona pratica eliminarla esplicitamente.

            string qryCancSql = $"DROP TEMPORARY TABLE IF EXISTS {nomeTabStaging};";

            // Eseguiamo questo comando indipendentemente dall'esito precedente, ma senza che un suo fallimento
            // (improbabile per IF EXISTS) alteri il risultato dell'operazione di upsert principale.

            try
            {
                await SqlNoQry(strConn, qryCancSql, timeoutSec);
            }
            catch (Exception exDrop)
            {
                Console.WriteLine($"Avviso: Fallimento nel tentativo di eliminare esplicitamente la tabella di staging {nomeTabStaging}: {exDrop.Message}");
            }
        }
    }
}
