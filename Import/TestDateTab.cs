namespace PvPmo.Import
{
    public partial class TestDateTab
    {
        public static async Task<bool> VerificaDateProduzioneUpdateAsync(IProgress<string>? progress)
        {
            string _connProd = await Conn.MysqlConn("pmo");
            string _connUptd = await Conn.MysqlConn("pvpmo_origine");

            if (string.IsNullOrEmpty(_connProd) || string.IsNullOrEmpty(_connUptd))
            {
                await Shell.Current.DisplayAlert("Errore Connessione", "Non è possibile creare la connessione al database.", "OK");
                return false;
            }

            try
            {
                var prodPvTotal = await LeggiDateDistinteAsync(_connProd, "pv_total");
                var uptdPvTotal = await LeggiDateDistinteAsync(_connUptd, "pv_total");

                var prodGTE = await LeggiDateDistinteAsync(_connProd, "global_timesheet_extract");
                var uptdGTE = await LeggiDateDistinteAsync(_connUptd, "global_timesheet_extract");

                progress?.Report($"Test Date tabelle importate produzione");

                // Validazione Tabella 1
                if (uptdPvTotal.Count != 1)
                {
                    await MostraErrore("Tabella pv_total update deve contenere una sola data.");
                    return false;
                }

                var maxProd1 = prodPvTotal.Max();
                var dataUpd1 = uptdPvTotal[0];

                var attesa1 = new DateTime(maxProd1.Year, maxProd1.Month, 1);
                var attesa2 = attesa1.AddMonths(1);

                if (dataUpd1 != attesa1 && dataUpd1 != attesa2)
                {
                    await MostraErrore($"Tabella 1 update deve contenere la data {attesa1:dd/MM/yyyy} o {attesa2:dd/MM/yyyy}.");
                    return false;
                }

                // Validazione Tabella 2
                if (uptdGTE.Count != 2)
                {
                    await MostraErrore("Tabella global_timesheet_extract update deve contenere esattamente due date.");
                    return false;
                }

                var distinteProd2 = new HashSet<DateTime>(prodGTE.Select(d => d.Date));
                var nuove = uptdGTE.Where(d => !distinteProd2.Contains(d.Date)).ToList();

                if (nuove.Count > 1)
                {
                    await MostraErrore("Tabella global_timesheet_extract update può contenere al massimo una data nuova non presente in produzione.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await MostraErrore($"Errore durante la validazione: {ex.Message}");
                return false;
            }
        }
        public static async Task<List<DateTime>> LeggiDateDistinteAsync(string connStr, string tabella)
        {
            var result = new List<DateTime>();
            string query = $"SELECT DISTINCT Dateid FROM {tabella} ORDER BY Dateid ASC;";

            using var conn = new MySqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                    result.Add(reader.GetDateTime(0));
            }

            return result;
        }
        public static async Task MostraErrore(string messaggio)
        {
            await Shell.Current.DisplayAlert("Errore Validazione Date", messaggio, "OK");
        }
    }
}
