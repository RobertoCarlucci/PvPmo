namespace PvPmo.GestDb;

public partial class SqlAsync
{
    public static List<MySqlBulkCopyColumnMapping> Mappings = new List<MySqlBulkCopyColumnMapping>();
    public static List<MySqlParameter> Params = new List<MySqlParameter>();

    public static void AddParam(string nome, Object vale)
    {
        var NewParam = new MySqlParameter(nome, vale);
        Params.Add(NewParam);
    }
    public static void AddMapping(int sourceOrdinal, string destinationColumn)
    {
        var NewMapping = new MySqlBulkCopyColumnMapping(sourceOrdinal, destinationColumn);
        Mappings.Add(NewMapping);
    }        
    public static async Task<Boolean> SqlNoQry(string strConn, string qry, int time)
    {
        try
        {
            var _connSql = new MySqlConnection(strConn);
            await _connSql.OpenAsync();
            var _cmdSql = new MySqlCommand(qry, _connSql);
            _cmdSql.CommandTimeout = time;
            Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
            Params.Clear();
            await _cmdSql.ExecuteNonQueryAsync();
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

    public static async Task<DataTable> SqlQryDataReader(string strConn, string qry, DataTable _tabella, int time)
    {
        try
        {
            var _connSql = new MySqlConnection(strConn);
            await _connSql.OpenAsync();
            var _cmdSql = new MySqlCommand(qry, _connSql);
            _cmdSql.CommandTimeout = time;
            Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
            Params.Clear();
            MySqlDataReader _datareader = _cmdSql.ExecuteReader();
            _tabella.Load(_datareader);
            _datareader.Close();
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
    public static async Task<DataTable> SqlQryDataTable(string strConn, string qry, DataTable _tabella, int time)
    {
        try
        {
            var _connSql = new MySqlConnection(strConn);
            await _connSql.OpenAsync();
            var _cmdSql = new MySqlCommand(qry, _connSql);
            _cmdSql.CommandTimeout = time;
            Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
            Params.Clear();
            var _adapter = new MySqlDataAdapter(_cmdSql);
            int _contarecord = _adapter.Fill(_tabella);
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
    public static async Task<Boolean> SqlBulkCopy(string strConn, string tabMod, DataTable tabellain, int time)
    {
        try
        {
            var _connSql = new MySqlConnection(strConn + "AllowLoadLocalInfile=true;");
            
            await _connSql.OpenAsync();
            var _bulk = new MySqlBulkCopy(_connSql);            
            _bulk.BulkCopyTimeout = time;
            _bulk.DestinationTableName = tabMod;
            Mappings.ForEach(_mapping => { _bulk.ColumnMappings.Add(_mapping); });
            Mappings.Clear();
            var result = await _bulk.WriteToServerAsync(tabellain);
            //if (result.Warnings.Count != 0);
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
