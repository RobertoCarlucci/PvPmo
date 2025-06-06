namespace PvPmo.GestDb;

public partial class SqlAsync
{    
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
            {
                Mappings.ForEach(_mapping => { bulk.ColumnMappings.Add(_mapping); });
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
