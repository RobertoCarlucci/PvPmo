namespace PvPmo.Db
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
    }
}  
