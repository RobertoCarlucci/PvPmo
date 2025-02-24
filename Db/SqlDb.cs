using System.Threading;

namespace PvPmo.Db
{
    public partial class SqlDb
    {
        private static List<MySqlParameter> Params = new List<MySqlParameter>();

        public static void AddParam(string nome, Object vale)
        {
            var NewParam = new MySqlParameter(nome, vale);
            Params.Add(NewParam);
        }

        public static DataTable SqlQrySyn(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connSql = new MySqlConnection(StrConn);
                _connSql.Open();
                var _cmdSql = new MySqlCommand(Qry, _connSql);
                var _adapter = new MySqlDataAdapter(_cmdSql);
                int ContaRecord = _adapter.Fill(_tabella);
                //int ContaRecord = _tabella.Rows.Count;
                return _tabella;
            }
            catch (MySqlException ex)
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
        public static async Task<DataTable> SqlQry(string StrConn, string Qry, DataTable _tabella)
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
        public static async Task<Boolean> SqlBulkCopy(string tabMod, string StrConn, DataTable tabellain)
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
        public static Boolean SqlNoQry(string Qry, string strConn)
        {            
            try
            {
                var _connSql = new MySqlConnection(strConn);
                _connSql.Open();
                var _cmdSql = new MySqlCommand(Qry, _connSql);
                Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
                Params.Clear();
                //Cmd.Prepare();
                _cmdSql.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException ex)
            {
                Shell.Current.DisplayAlert
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
