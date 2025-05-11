namespace PvPmo.GestDb
{
    public class ExlAsync
    {
        private static OleDbConnection conn;
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
                    int count = adapter.Fill(tabella);
                    return count;
                });
            }
            catch (OleDbException ex)
            {
                // gestisci a livello superiore (es: logger/VM)
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
                // gestisci a livello superiore (es: logger/VM)
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
    }
}
