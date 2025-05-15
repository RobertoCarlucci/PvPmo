namespace PvPmo.GestDb
{
    public class AcsAsync
    {
        private static OleDbConnection? conn; // Marked as nullable
        private static DataTable? _tabella;
        public static async Task<DataTable> AcsQryTab(string strConn, string qry, DataTable tabella)
        {
            try
            {
                DataTable _tabella = new DataTable();
                conn = new OleDbConnection(strConn); // Assigning a value to the static field
                conn.Open();
                var cmd = new OleDbCommand(qry, conn);
                var _adapter = new OleDbDataAdapter(cmd);
                int ContaRecord = _adapter.Fill(_tabella);
                return _tabella;
            }
            catch (OleDbException ex)
            {
                await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Access");
                return _tabella;
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
