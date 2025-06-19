namespace PvPmo.GestDb;

public partial class SqlAsync
{
    public static async Task<bool> TestConnSql(string stringaDiConnessione)
    {
        // Il blocco 'await using' garantisce che la connessione venga
        // chiusa e le risorse rilasciate, anche in caso di errore.
        await using var connection = new MySqlConnection(stringaDiConnessione);

        try
        {
            // 1. Tenta di aprire la connessione in modo asincrono.
            // Questa è la riga che esegue il test vero e proprio.
            await connection.OpenAsync();

            // Se il codice arriva qui, l'apertura ha avuto successo.
            // 2. La connessione viene chiusa automaticamente alla fine del blocco 'using'.
            return true;
        }
        catch (MySqlException ex)
        {
            await DbErrorHandler.ShowErrorAsync(ex, "Test Connessione");
            return false;
        }
    }
    public static async Task<bool> SqlNoQry(
        MySqlConnection conn, string qry, int timeoutSec = 0, List<MySqlParameter>? parameters = null)
    {        
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
        MySqlConnection conn, string tableName, DataTable data, List<MySqlBulkCopyColumnMapping>? Mappings = null, int timeoutSec = 0)
    {
        //conn += "AllowLoadLocalInfile=true;";
        //await using var conn = new MySqlConnection(strConn);
        try
        {
            await conn.OpenAsync();
            var bulk = new MySqlBulkCopy(conn)
            {
                BulkCopyTimeout = timeoutSec,
                DestinationTableName = tableName
            };
            if (Mappings != null)
            {
                Mappings.ForEach(bulk.ColumnMappings.Add);
                Mappings.Clear(); // This line is safe now because we check for null above
            }

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
            {
                Mappings.ForEach(bulk.ColumnMappings.Add);
                Mappings.Clear(); // This line is safe now because we check for null above
            }

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
}
