namespace PvPmo.Util
{
    public partial class TestDate
    {
        public static async Task<bool> VerificaDateDaTabellaAsync(
            string connStr, string nomeTabella)
        {
            var col1 = new List<DateTime>();
            var col2 = new List<DateTime>();
            var col3 = new List<DateTime>();

            string query = $"SELECT PVTotalData, GlobalTimesheetExtractData, PVMeseSuAnno FROM {nomeTabella}";

            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                        col1.Add(Convert.ToDateTime(reader.GetValue(0)));

                    if (!reader.IsDBNull(1))
                        col2.Add(Convert.ToDateTime(reader.GetValue(1)));

                    if (!reader.IsDBNull(2))
                        col3.Add(Convert.ToDateTime(reader.GetValue(2)));
                }
            }
            catch (MySqlException ex)
            {
                await DbErrorHandler.ShowErrorAsync(ex, "Test Connessione");
                return false;
                
            }

            if (!VerificaStrutturaDate(col1, col2, col3, out var errore))
                return false;

            return true;
        }


        public static bool VerificaStrutturaDate(
                List<DateTime> col1, List<DateTime> col2, List<DateTime> col3, out string errore)
        {
            errore = string.Empty;

            if (col1.Count != 1)
            {
                errore = "❌ Colonna 1 deve contenere una sola data.";
                return false;
            }

            var dataRif = col1[0];

            if (col2.Count != 2)
            {
                errore = "❌ Colonna 2 deve contenere esattamente due date.";
                return false;
            }

            var attesa1 = new DateTime(dataRif.Year, dataRif.Month, 1).AddMonths(-1);
            var attesa2 = new DateTime(dataRif.Year, dataRif.Month, 1);

            var col2Ordinate = col2.OrderBy(d => d).ToList();
            if (col2Ordinate[0] != attesa1 || col2Ordinate[1] != attesa2)
            {
                errore = $"❌ Colonna 2 deve contenere: {attesa1:dd/MM/yyyy}, {attesa2:dd/MM/yyyy}";
                return false;
            }

            if (col3.Count == 0)
            {
                errore = "❌ Colonna 3 deve contenere almeno una data.";
                return false;
            }

            var attesa = dataRif.AddMonths(1);
            foreach (var d in col3.OrderBy(d => d))
            {
                if (d != attesa)
                {
                    errore = $"❌ Colonna 3: atteso {attesa:dd/MM/yyyy}, trovato {d:dd/MM/yyyy}";
                    return false;
                }
                attesa = attesa.AddMonths(1);
            }

            return true;
        }
    }
}
