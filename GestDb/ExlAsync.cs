namespace PvPmo.GestDb
{
    public class ExlAsync
    {
        private static OleDbConnection? conn;
        
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

        public static async Task<bool> ExcQry(
            string strConn,
            string qry,
            DataTable tabellain,
            IProgress<(double, string)>? progress = null,
            string? label = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var conn = new OleDbConnection(strConn);
                    using var cmd = new OleDbCommand(qry, conn);
                    using var adapter = new OleDbDataAdapter(cmd);
                    conn.Open();

                    var temp = new DataTable();
                    adapter.Fill(temp);

                    int total = temp.Rows.Count;
                    for (int i = 0; i < total; i++)
                    {
                        tabellain.ImportRow(temp.Rows[i]);

                        if (i % 100 == 0) // ogni 100 righe aggiorna la progress
                        {
                            double percent = i / (double)total;
                            progress?.Report((percent, $"Read: {label ?? "Excel"}"));
                        }
                    }

                    progress?.Report((1.0, $"Read: {label ?? "Excel"}"));
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

        //public static async Task<bool> ExcQry(string strConn, string qry, DataTable tabellain)
        //{
        //    try
        //    {
        //        return await Task.Run(() =>
        //        {
        //            using var conn = new OleDbConnection(strConn);
        //            using var cmd = new OleDbCommand(qry, conn);
        //            using var adapter = new OleDbDataAdapter(cmd);
        //            conn.Open();
        //            adapter.Fill(tabellain);
        //            return true;
        //        });
        //    }
        //    catch (OleDbException ex)
        //    {
        //        await DbErrorHandler.ShowOleDbErrorAsync(ex, "Importazione Excel");
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
    }
}
