namespace PvPmo.GestDb
{
    public class ExlAsync
    {
        public static async Task<bool> TestConnExl(string strConn)
        {
            await using var connection = new OleDbConnection(strConn);

            try
            {
                // Il principio è identico: tentiamo di aprire la connessione.
                await connection.OpenAsync();

                return true;
            }
            catch (OleDbException ex)
            {
                await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Access");
                throw;
            }
        }
        public static async Task<int> ExcQry(string strConn, string qry)
        {
            using var conn = new OleDbConnection(strConn);

            try
            {
                return await Task.Run(() =>
                {                    
                    using var cmd = new OleDbCommand(qry, conn);
                    using var adapter = new OleDbDataAdapter(cmd);
                    var tabella = new DataTable();
                    conn.Open();
                    int count = adapter.Fill(tabella);
                    return count;
                });
            }
            catch (OleDbException ex)
            {
                await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Excel");
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
        public static async Task<bool> ExcQry(string strConn, string qry, DataTable tabellain)
        {
            using var conn = new OleDbConnection(strConn);

            try
            {
                return await Task.Run(() =>
                {                    
                    using var cmd = new OleDbCommand(qry, conn);
                    using var adapter = new OleDbDataAdapter(cmd);
                    conn.Open();
                    adapter.Fill(tabellain);
                    return true;
                });
            }
            catch (OleDbException ex)
            {
                await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Excel");
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
}
