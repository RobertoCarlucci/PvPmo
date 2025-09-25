namespace PvPmo.Util
{
    public static class DbErrorHandler
    {
        public static async Task ShowErrorAsync(MySqlException ex, string? context = null)
        {
            var title = "Errore Database";
            var msg = context != null
                ? $"Errore durante: {context}\nCodice: {ex.Number}\nMessaggio: {ex.Message}"
                : $"Codice: {ex.Number}\nMessaggio: {ex.Message}";

            await Shell.Current.DisplayAlert(title, msg, "OK");

            await LogManager.LogAsync("MySQL_ERROR", $"[{context}] Codice: {ex.Number} - {ex.Message}");
        }

        public static async Task ShowOleDbErrorAsync(OleDbException ex, string? context = null)
        {
            var title = "Errore Access/Excel (OleDb)";
            var msg = context != null
                ? $"Errore durante: {context}\nCodice: {ex.ErrorCode}\nMessaggio: {ex.Message}"
                : $"Codice: {ex.ErrorCode}\nMessaggio: {ex.Message}";

            await Shell.Current.DisplayAlert(title, msg, "OK");

            await LogManager.LogAsync("OLEDB_ERROR", $"[{context}] Codice: {ex.ErrorCode} - {ex.Message}");
        }
    }
}