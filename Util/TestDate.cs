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
                await DbErrorHandler.ShowErrorAsync(ex, "Caricamento Classe TestDate Tabella 01_tabella_data");
                return false;                
            }
            if (!VerificaStrutturaDate(col1, col2, col3))
                return false;

            return true;
        }
        public static bool VerificaStrutturaDate(
                List<DateTime> col1, List<DateTime> col2, List<DateTime> col3)
        {            
            if (col1.Count != 1)
            {
                Shell.Current.DisplayAlert("Test Date Inserite !", "Colonna PV_TotalData deve contenere una sola data.", "OK");
                return false;
            }

            var dataRif = col1[0];

            if (col2.Count != 2)
            {
                Shell.Current.DisplayAlert("Test Date Inserite !", "Colonna GlobalTimesheetExtractData deve contenere esattamente due date.", "OK");                
                return false;
            }

            var attesa1 = new DateTime(dataRif.Year, dataRif.Month, 1).AddMonths(-1);
            var attesa2 = new DateTime(dataRif.Year, dataRif.Month, 1);

            var col2Ordinate = col2.OrderBy(d => d).ToList();
            if (col2Ordinate[0] != attesa1 || col2Ordinate[1] != attesa2)
            {
                Shell.Current.DisplayAlert("Test Date Inserite !", "Colonna PV_TotalData deve contenere:" +
                    $"\n{attesa1:dd/MM/yyyy}, e non  {attesa2:dd/MM/yyyy}." +
                    $"\nDevi controllare i valori inseriti tra: PV_TotalData e GlobalTimesheetExtractData", "OK");                
                return false;
            }

            if (col3.Count == 0)
            {
                Shell.Current.DisplayAlert("Test Date Inserite !", "Colonna PVMeseSuAnno deve contenere almeno una data.", "OK");                
                return false;
            }

            var attesa = dataRif.AddMonths(1);
            foreach (var d in col3.OrderBy(d => d))
            {
                if (d != attesa)
                {
                    Shell.Current.DisplayAlert("Test Date Inserite !", $"Colonna PVMeseSuAnno atteso {attesa:dd/MM/yyyy}, trovato {d:dd/MM/yyyy}.", "OK");                    
                    return false;
                }
                attesa = attesa.AddMonths(1);
            }
            return true;
        }
    }
}
