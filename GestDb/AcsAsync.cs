namespace PvPmo.GestDb
{
    public class AcsAsync
    {
        public static async Task<DataTable> AcsQryTab(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connAcs = new OleDbConnection(StrConn);
                _connAcs.Open();
                var _cmdAcs = new OleDbCommand(Qry, _connAcs);                
                var _adapter = new OleDbDataAdapter(_cmdAcs);
                //await Task.Run(() => _adapter.Fill(_tabella));                
                int ContaRecord = _adapter.Fill(_tabella);
                return _tabella;
            }
            catch (OleDbException ex)
            {
                await Shell.Current.DisplayAlert
                    ("Errore MariaDb", $"Codice: {ex}", "Ok");
                return _tabella;
            }
            finally
            {
                if (ConnectionState.Open != ConnectionState.Closed) { };
            }
        }
        public static async Task<DataTable> AcsQryDataReader(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connAcs = new OleDbConnection(StrConn);
                _connAcs.Open();
                string[] restrictions = new string[4];
                var _cmdAcs = new OleDbCommand(Qry, _connAcs);
                OleDbDataReader _datareader = _cmdAcs.ExecuteReader();
                _tabella.Load(_datareader);
                _datareader.Close();                
                return _tabella;
            }
            catch (OleDbException ex)
            {
                await Shell.Current.DisplayAlert
                    ("Errore MariaDb", $"Codice: {ex}", "Ok");
                return _tabella;
            }
            finally
            {
                if (ConnectionState.Open != ConnectionState.Closed) { };
            }
        }
        //public static async Task<DataTable> AcsQryDataReader(string StrConn, string Qry, DataTable _tabella)
        //{
        //    try
        //    {
        //        var _connAcs = new OleDbConnection(StrConn);
        //        _connAcs.Open();
        //        string[] restrictions = new string[4];
        //        var _cmdAcs = new OleDbCommand(Qry, _connAcs);
        //        OleDbDataReader _datareader = _cmdAcs.ExecuteReader();
        //        _tabella.Load(_datareader);
        //        _datareader.Close();
        //        return _tabella;
        //    }
        //    catch (OleDbException ex)
        //    {
        //        await Shell.Current.DisplayAlert
        //            ("Errore MariaDb", $"Codice: {ex}", "Ok");
        //        return _tabella;
        //    }
        //    finally
        //    {
        //        if (ConnectionState.Open != ConnectionState.Closed) { };
        //    }
        //}
        public static async Task<bool> AcsDelifExist(string StrConn, string tableDel)
        {
            string tableToDelete = tableDel;   //table name
            bool tableExists = false;
            try
            {
                var _connAcs = new OleDbConnection(StrConn);
                _connAcs.Open();
                DataTable dt = _connAcs.GetSchema("tables");
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
                    using (OleDbCommand cmd = new OleDbCommand(string.Format("DROP TABLE {0}", tableToDelete), _connAcs))
                    {
                        cmd.ExecuteNonQuery();                        
                    }
                }
                return tableExists;
            }
            catch (OleDbException ex)
            {
                await Shell.Current.DisplayAlert
                    ("Errore MariaDb", $"Codice: {ex}", "Ok");
                return tableExists;
            }
            finally
            {
                if (ConnectionState.Open != ConnectionState.Closed) { };
            }
        }
    }
}  
