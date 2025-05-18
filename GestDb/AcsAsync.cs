namespace PvPmo.GestDb
{
    public class AcsAsync
    {
        private static OleDbConnection? conn; // Marked as nullable

        public static async Task<bool> AcsQryTab(
            string strConn, string qry, DataTable tabellain,
            IProgress<(double, string)>? progress = null, string? tableName = null)
        {
            try
            {
                using var conn = new OleDbConnection(strConn);
                await conn.OpenAsync();
                var cmd = new OleDbCommand(qry, conn);
                var adapter = new OleDbDataAdapter(cmd);

                var tempTable = new DataTable();
                adapter.Fill(tempTable);

                int total = tempTable.Rows.Count;
                for (int i = 0; i < total; i++)
                {
                    tabellain.ImportRow(tempTable.Rows[i]);
                    if (i % 50 == 0)
                        progress?.Report((i / (double)total, $"Read: {tableName}"));
                }

                progress?.Report((1, $"Read: {tableName}")); // completato
                return true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore Access", ex.Message, "OK");
                return false;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    await conn.CloseAsync();
                }
            }
        }


        //public static async Task<DataTable> AcsQryTab(string strConn, string qry, DataTable tabella)
        //{
        //    try
        //    {                
        //        conn = new OleDbConnection(strConn); // Assigning a value to the static field
        //        conn.Open();
        //        var cmd = new OleDbCommand(qry, conn);
        //        var _adapter = new OleDbDataAdapter(cmd);
        //        int ContaRecord = _adapter.Fill(tabella);
        //        return tabella;
        //    }
        //    catch (OleDbException ex)
        //    {
        //        await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Access");
        //        throw;
        //    }
        //    finally
        //    {
        //        if (conn != null && conn.State == ConnectionState.Open)
        //        {
        //            await conn.CloseAsync();
        //        }
        //    }
        //}
    }
}  
