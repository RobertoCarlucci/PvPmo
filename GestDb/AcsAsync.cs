namespace PvPmo.GestDb
{
    public class AcsAsync
    {
        private static OleDbConnection conn;
        public static async Task<DataTable> AcsQryTab(string strConn, string qry, DataTable tabella)
        {
            try
            {
                var conn = new OleDbConnection(strConn);
                conn.Open();
                var cmd = new OleDbCommand(qry, conn);                
                var _adapter = new OleDbDataAdapter(cmd);
                //await Task.Run(() => _adapter.Fill(_tabella));                
                int ContaRecord = _adapter.Fill(tabella);
                return tabella;
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
        public static async Task<DataTable> AcsQryDataReader(string strConn, string qry, DataTable tabella)
        {
            try
            {
                var conn = new OleDbConnection(strConn);
                conn.Open();
                string[] restrictions = new string[4];
                var cmd = new OleDbCommand(qry, conn);
                OleDbDataReader _datareader = cmd.ExecuteReader();
                tabella.Load(_datareader);
                _datareader.Close();                
                return tabella;
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
        
        public static async Task<bool> AcsDelifExist(string strConn, string tableDel)
        {
            string tableToDelete = tableDel;   //table name
            bool tableExists = false;
            try
            {
                var conn = new OleDbConnection(strConn);
                conn.Open();
                DataTable dt = conn.GetSchema("tables");
                foreach (DataRow row in dt.Rows)
                {
                    if (row["TABLE_NAME"].ToString() == tableToDelete)
                    {
                        tableExists = true;
                        break;
                    }
                }
                if (tableExists)
                {
                    using (OleDbCommand cmd = new OleDbCommand(string.Format("DROP TABLE {0}", tableToDelete), conn))
                    {
                        cmd.ExecuteNonQuery();                        
                    }
                }
                return tableExists;
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
