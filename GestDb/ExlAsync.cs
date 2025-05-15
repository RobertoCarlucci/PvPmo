namespace PvPmo.GestDb
{
    public class ExlAsync
    {
        private static OleDbConnection? conn;
        private static int count;
        public static async Task<int> ExcQry(string strConn, string qry)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var conn = new OleDbConnection(strConn);
                    using var cmd = new OleDbCommand(qry, conn);
                    using var adapter = new OleDbDataAdapter(cmd);
                    var tabella = new DataTable();
                    conn.Open();
                    count = adapter.Fill(tabella);
                    return count;
                });
            }
            catch (OleDbException ex)
            {
                await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Excel");
                return count = 0;
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
            try
            {
                return await Task.Run(() =>
                {
                    using var conn = new OleDbConnection(strConn);
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
