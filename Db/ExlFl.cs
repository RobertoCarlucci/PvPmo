using System.Runtime.InteropServices;

namespace PvPmo.Db
{
    public class ExlFl
    {
        public static int ExcQry(string Qry, string StrConn)
        {            
            try
            {
                var _connExl = new OleDbConnection(StrConn);
                _connExl.Open();
                var _cmdExl = new OleDbCommand(Qry, _connExl);
                var _tabella = new DataTable();
                var _adapter = new OleDbDataAdapter(_cmdExl);
                var ContaRecord = _adapter.Fill(_tabella);
                return ContaRecord;
            }
            catch (OleDbException ex)
            {
                Shell.Current.DisplayAlert
                    ("Errore MariaDb", $"Codice: {ex}", "Ok");                
                return -1;
            }
            finally
            {
                if (ConnectionState.Open != ConnectionState.Closed) { };
            }
        }
        public static Boolean ExcQry(string Qry, string StrConn, DataTable tabellain)
        {           
            try
            {
                var _connExl = new OleDbConnection(StrConn);
                _connExl.Open();
                var _cmdExl = new OleDbCommand(Qry, _connExl);
                var _adapter = new OleDbDataAdapter(_cmdExl);
                var ContaRecord = _adapter.Fill(tabellain);
                return true;
            }
            catch (OleDbException ex)
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
