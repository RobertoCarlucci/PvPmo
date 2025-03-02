namespace PvPmo.Db
{
    public class AcsDb
    {
        public static async Task<DataTable> AcsQryTab(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connAcs = new OleDbConnection(StrConn);
                _connAcs.Open();
                var _cmdAcs = new OleDbCommand(Qry, _connAcs);                
                var _adapter = new OleDbDataAdapter(_cmdAcs);
                await Task.Run(() => _adapter.Fill(_tabella));                
                //ContaRecord = _adapter.Fill(_tabella);
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
                var _adapter = new OleDbDataAdapter(_cmdAcs);
                await Task.Run(() => _adapter.Fill(_tabella));
                //ContaRecord = _adapter.Fill(_tabella);
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
                if (ConnectionState.Open != ConnectionState.Closed) { }
                ;
            }
        }
    }
}  
