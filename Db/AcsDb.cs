namespace PvPmo.Db
{
    public class AcsDb
    {
        public static async Task<DataTable> AcsQryTab(string Qry, string StrConn, DataTable _tabella)
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
                Shell.Current.DisplayAlert
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
