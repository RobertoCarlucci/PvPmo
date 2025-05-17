namespace PvPmo.GestDb;

public partial class SqlAsync
{
    public static List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
    //public static List<MySqlParameter> Params = new List<MySqlParameter>();
    private static MySqlConnection? conn;

    //public static void AddParam(string nome, Object vale)
    //{
    //    var NewParam = new MySqlParameter(nome, vale);
    //    Params.Add(NewParam);
    //}
    public static void AddMapping(int sourceOrdinal, string destinationColumn)
    {
        var NewMapping = new MySqlBulkCopyColumnMapping(sourceOrdinal, destinationColumn);
        Mappings.Add(NewMapping);
    }
    public static async Task<bool> SqlNoQry(string strConn, string qry, int timeoutSec = 30, List<MySqlParameter>? parameters = null)
    {
        try
        {
            await using var conn = new MySqlConnection(strConn);
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
    public static async Task<DataTable> SqlQryDataTable(string strConn, string qry, DataTable tabella, int timeoutSec = 30, List<MySqlParameter>? parameters = null)
    {
        try
        {
            await using var conn = new MySqlConnection(strConn);
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
    string strConn,
    string tableName,
    DataTable data,
    int timeoutSec = 30,
    IProgress<(double, string)>? progress = null)
    {
        try
        {
            strConn += "AllowLoadLocalInfile=true;";
            await using var conn = new MySqlConnection(strConn);
            await conn.OpenAsync();

            var bulk = new MySqlBulkCopy(conn)
            {
                BulkCopyTimeout = timeoutSec,
                DestinationTableName = tableName
            };

            Mappings.ForEach(m => bulk.ColumnMappings.Add(m));
            Mappings.Clear();

            int totalRows = data.Rows.Count;
            int batchSize = 500;
            int copied = 0;

            for (int offset = 0; offset < totalRows; offset += batchSize)
            {
                var batch = data.Clone();
                int toCopy = Math.Min(batchSize, totalRows - offset);
                for (int i = 0; i < toCopy; i++)
                    batch.ImportRow(data.Rows[offset + i]);

                await bulk.WriteToServerAsync(batch);
                copied += toCopy;

                double percent = copied / (double)totalRows;
                progress?.Report((percent, tableName));
            }

            return true;
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



    //public static async Task<bool> SqlBulkCopy(string strConn, string tableName, DataTable data, int timeoutSec = 30)
    //{
    //    try
    //    {
    //        strConn += "AllowLoadLocalInfile=true;";
    //        await using var conn = new MySqlConnection(strConn);
    //        await conn.OpenAsync();

    //        var bulk = new MySqlBulkCopy(conn)
    //        {
    //            BulkCopyTimeout = timeoutSec,
    //            DestinationTableName = tableName
    //        };

    //        Mappings.ForEach(_mapping => { bulk.ColumnMappings.Add(_mapping); });
    //        Mappings.Clear();

    //        //if (mappings != null)
    //        //    mappings.ForEach(m => bulk.ColumnMappings.Add(m));

    //        await bulk.WriteToServerAsync(data);
    //        return true;
    //    }
    //    catch (MySqlException ex)
    //    {
    //        await DbErrorHandler.ShowErrorAsync(ex, "Esecuzione SQL");
    //        return false;
    //    }
    //    finally
    //    {
    //        if (conn != null && conn.State == ConnectionState.Open) // Fixed condition
    //        {
    //            await conn.CloseAsync();
    //        }
    //    }
    //}
    //public static async Task<bool> SqlBulkCopy(
    //string strConn,
    //string tableName,
    //DataTable data,
    //int timeoutSec = 30,
    //IProgress<double>? progress = null)
    //{
    //    try
    //    {
    //        strConn += "AllowLoadLocalInfile=true;";
    //        await using var conn = new MySqlConnection(strConn);
    //        await conn.OpenAsync();

    //        var bulk = new MySqlBulkCopy(conn)
    //        {
    //            BulkCopyTimeout = timeoutSec,
    //            DestinationTableName = tableName
    //        };

    //        Mappings.ForEach(m => bulk.ColumnMappings.Add(m));
    //        Mappings.Clear();

    //        // Simulazione avanzamento manuale (step = 10 righe)
    //        int totalRows = data.Rows.Count;
    //        int batchSize = 500; // ogni quanto notificare

    //        for (int offset = 0; offset < totalRows; offset += batchSize)
    //        {
    //            int toCopy = Math.Min(batchSize, totalRows - offset);
    //            DataTable batch = data.Clone();
    //            for (int i = 0; i < toCopy; i++)
    //                batch.ImportRow(data.Rows[offset + i]);

    //            await bulk.WriteToServerAsync(batch);

    //            double percent = (double)(offset + toCopy) / totalRows;
    //            progress?.Report(percent);
    //        }

    //        return true;
    //    }
    //    catch (MySqlException)
    //    {
    //        throw;
    //    }
    //    finally
    //    {
    //        if (conn != null && conn.State == ConnectionState.Open) // Fixed condition
    //        {
    //            await conn.CloseAsync();
    //        }
    //    }
    //}

}
