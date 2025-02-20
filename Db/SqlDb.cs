using System.Threading;

namespace PvPmo.Db
{
    public partial class SqlDb
    {        
        public static async Task<DataTable> QuerySemplice(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connSql = new MySqlConnection(StrConn);
                await _connSql.OpenAsync();
                var _cmdSql = new MySqlCommand(Qry, _connSql);                
                var _adapter = new MySqlDataAdapter(_cmdSql);                
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
        public static async Task<Boolean> EseguiBulk(string tabMod, string StrConn, DataTable tabellain)
        {            
            try
            {
                var _connSql = new MySqlConnection(StrConn + "AllowLoadLocalInfile=true;");                
                await _connSql.OpenAsync();                
                var _bulk = new MySqlBulkCopy(_connSql);
                var _cmdSql = new MySqlCommand(tabMod, _connSql);
                _bulk.DestinationTableName = tabMod;
                var result = await _bulk.WriteToServerAsync(tabellain);
                //if (result.Warnings.Count != 0) { "Possibile" };
                return true;
            }
            catch (MySqlException ex)
            {
                await Shell.Current.DisplayAlert
                    ("Errore MariaDb", $"Codice: {ex}", "Ok");
                return false;
            }
            finally
            {
                if (ConnectionState.Open != ConnectionState.Closed) { };
            }
        }
    }
}
