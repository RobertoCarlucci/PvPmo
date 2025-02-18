namespace PvPmo.Db
{
    public partial class Database
    {        
        public static async Task<DataTable> QuerySemplice(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connSql = new MySqlConnection(StrConn);
                await _connSql.OpenAsync();
                var _cmdSql = new MySqlCommand(Qry, _connSql);                
                var _adapter = new MySqlDataAdapter(_cmdSql);
                //var _tabella = new DataTable();
                await Task.Run(() => _adapter.Fill(_tabella));                
                int ContaRecord = _tabella.Rows.Count;
                return _tabella;
            }
            catch (MySqlException ex)
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
