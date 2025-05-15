namespace PvPmo.GestDb
{
    public class AcsAsync
    {
        private static OleDbConnection? conn; // Marked as nullable
        
        public static async Task<DataTable> AcsQryTab(string strConn, string qry, DataTable tabella)
        {
            try
            {                
                conn = new OleDbConnection(strConn); // Assigning a value to the static field
                conn.Open();
                var cmd = new OleDbCommand(qry, conn);
                var _adapter = new OleDbDataAdapter(cmd);
                int ContaRecord = _adapter.Fill(tabella);
                return tabella;
            }
            catch (OleDbException ex)
            {
                await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Access");
                throw;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    await conn.CloseAsync();
                }
            }
        }
    }
}  
