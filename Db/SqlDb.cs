using System.Threading;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PvPmo.Db
{
    public partial class SqlDb
    {
        public static List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
        public static List<MySqlParameter> Params = new List<MySqlParameter>();

        public static void AddParam(string nome, Object vale)
        {
            var NewParam = new MySqlParameter(nome, vale);
            Params.Add(NewParam);
        }
        public static void AddMapping(int SourceOrdinal, string DestinationColumn)
        {
            var NewMapping = new MySqlBulkCopyColumnMapping(SourceOrdinal, DestinationColumn);
            Mappings.Add(NewMapping);
        }

        public static DataTable SqlQrySyn(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connSql = new MySqlConnection(StrConn);
                _connSql.Open();
                var _cmdSql = new MySqlCommand(Qry, _connSql);
                Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
                Params.Clear();
                var _adapter = new MySqlDataAdapter(_cmdSql);
                int ContaRecord = _adapter.Fill(_tabella);                
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
        public static bool SqlQrySyn(string StrConn, string Qry)
        {
            try
            {
                var _connSql = new MySqlConnection(StrConn);
                _connSql.Open();
                var _cmdSql = new MySqlCommand(Qry, _connSql);
                Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
                Params.Clear();
                MySqlDataReader _lettore = _cmdSql.ExecuteReader();
                //int ContaRecord = _a;
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
                if (ConnectionState.Open != ConnectionState.Closed) { }
                ;
            }
        }

        public static async Task<DataTable> SqlQry(string StrConn, string Qry, DataTable _tabella)
        {
            try
            {
                var _connSql = new MySqlConnection(StrConn);
                await _connSql.OpenAsync();
                var _cmdSql = new MySqlCommand(Qry, _connSql);
                Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
                Params.Clear();
                var _adapter = new MySqlDataAdapter(_cmdSql);          
                               
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
                _bulk.DestinationTableName = tabMod;
                Mappings.ForEach(_mapping => { _bulk.ColumnMappings.Add(_mapping); });
                Mappings.Clear();                
                var result = await _bulk.WriteToServerAsync(tabellain);
                if (result.Warnings.Count != 0);
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
                //Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
                //Params.Clear();
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
