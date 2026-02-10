namespace PvPmo.Import;

public partial class ImpSupportTables
{
    public static async Task<bool> ImportSupportTables()
    {
        string? exlPath = await SelCart.PickFolder();
        if (string.IsNullOrWhiteSpace(exlPath))
        {
            await Shell.Current.DisplayAlert("Errore selezione cartella !", "Non è stata effettuata alcuna selezione.", "OK");
            return false;
        }

        bool allOk = true;
        bool Header = true; // Assuming the Excel files have headers, adjust if necessary    
        DataTable table;
        string fullPath;

        table = await ExcelImporter.ImportExcelToDataTableAsync(fullPath = $"{exlPath}\\DailyTable_Average_ManHours_Vertical.xlsx", "Calendario", Header);
        allOk = (table != null && table.Rows.Count > 0);
        table = await ExcelImporter.ImportExcelToDataTableAsync(fullPath = $"{exlPath}\\Date.xlsx", "Date", Header);
        allOk = (table != null && table.Rows.Count > 0);
        table = await ExcelImporter.ImportExcelToDataTableAsync(fullPath = $"{exlPath}\\FTE_Mese.xlsx", "Conversione ore-FTE per mese", Header);
        allOk = (table != null && table.Rows.Count > 0);
        table = await ExcelImporter.ImportExcelToDataTableAsync(fullPath = $"{exlPath}\\Org.xlsx", "Org", Header);
        allOk = (table != null && table.Rows.Count > 0);
        table = await ExcelImporter.ImportExcelToDataTableAsync(fullPath = $"{exlPath}\\ResourceType.xlsx", "Resource Type", Header);
        allOk = (table != null && table.Rows.Count > 0);
        table = await ExcelImporter.ImportExcelToDataTableAsync(fullPath = $"{exlPath}\\Version.xlsx", "Version", Header);
        allOk = (table != null && table.Rows.Count > 0);

        return allOk;
    }
}
