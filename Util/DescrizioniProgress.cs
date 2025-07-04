namespace PvPmo.Util;

public static class DescrizioniProgress
{
    private static Dictionary<string, string>? _cache;
    private static DateTime _lastLoaded = DateTime.MinValue;
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10); // ricarica ogni 10 min

    public static async Task<Dictionary<string, string>> GetProgressDescriptionsAsync(string connStr)
    {
        if (_cache != null && DateTime.Now - _lastLoaded < _cacheDuration)
            return _cache;

        var descrizioni = new Dictionary<string, string>();
        var tabella = new DataTable();
        string qry = "SELECT TabellaSql, DbName, Descrizione FROM progress_descrizione";

        try
        {
            await SqlAsync.SqlQryDataTable(connStr, qry, tabella);

            foreach (DataRow row in tabella.Rows)
            {
                string? tab = row["TabellaSql"]?.ToString();
                string? db = row["DbName"]?.ToString();
                string? desc = row["Descrizione"]?.ToString();
                if (!string.IsNullOrWhiteSpace(tab) && !string.IsNullOrWhiteSpace(db) && !string.IsNullOrWhiteSpace(desc))
                    descrizioni[$"{tab}|{db}"] = desc;
            }

            _cache = descrizioni;
            _lastLoaded = DateTime.Now;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore descrizioni", ex.Message, "OK");
        }

        return _cache ?? new Dictionary<string, string>();
    }
    public static void InvalidaCache()
    {
        _cache = null;
        _lastLoaded = DateTime.MinValue;
    }
}
