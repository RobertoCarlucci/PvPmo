namespace PvPmo.GestDb;

public partial class SqlSync
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

    public static DataTable SqlQryDataReader(string StrConn, string Qry, DataTable _tabella)
    {
        try
        {
            var _connSql = new MySqlConnection(StrConn);
            _connSql.Open();
            var _cmdSql = new MySqlCommand(Qry, _connSql);
            Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
            Params.Clear();
            MySqlDataReader _datareader = _cmdSql.ExecuteReader();
            _tabella.Load(_datareader);                
            _datareader.Close();
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
    public static bool SqlQry(string StrConn, string Qry)
    {
        try
        {
            var _connSql = new MySqlConnection(StrConn);
            _connSql.Open();
            var _cmdSql = new MySqlCommand(Qry, _connSql);
            Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
            Params.Clear();
            var _adapter = new MySqlDataAdapter(_cmdSql);
            //int ContaRecord = _adapter.Fill(_tabella);
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
    public static DataTable SqlQryDataTable(string StrConn, string Qry, DataTable _tabella)
    {
        try
        {
            var _connSql = new MySqlConnection(StrConn + "Convert Zero Datetime=True;");
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
    public static Boolean SqlNoQry(string strConn, string Qry)
    {            
        try
        {
            var _connSql = new MySqlConnection(strConn);
            _connSql.Open();
            var _cmdSql = new MySqlCommand(Qry, _connSql);
            Params.ForEach(param => { _cmdSql.Parameters.Add(param); });
            Params.Clear();               
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
    public static Boolean SqlBulkCopy(string StrConn, string tabMod, DataTable tabellain)
    {
        try
        {
            var _connSql = new MySqlConnection(StrConn + "AllowLoadLocalInfile=true;");
            _connSql.Open();
            var _bulk = new MySqlBulkCopy(_connSql);
            _bulk.DestinationTableName = tabMod;
            Mappings.ForEach(_mapping => { _bulk.ColumnMappings.Add(_mapping); });
            Mappings.Clear();
            var result = _bulk.WriteToServer(tabellain);
            //if (result.Warnings.Count != 0) ;
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
