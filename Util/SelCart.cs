using CommunityToolkit.Maui.Storage;

namespace PvPmo.Util
{
    public static class SelCart
    {
        public static async Task<string?> PickFolder()
        {
            try
            {
                var result = await FolderPicker.PickAsync("Seleziona cartella");

                if (result.IsSuccessful && !string.IsNullOrWhiteSpace(result.Folder.Path))
                    return result.Folder.Path;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore selezione cartella", ex.Message, "OK");
            }

            return null;
        }

        public static async Task<string?> PickFile()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();

                if (result is not null)
                {
                    var ext = Path.GetExtension(result.FileName).ToLowerInvariant();
                    if (ext is ".accdb" or ".xlsx")
                        return result.FullPath;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore selezione file", ex.Message, "OK");
            }

            return null;
        }
        public static async Task<List<string>> PickMultipleFilesAsync(string[] allowedExtensions = null!)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Seleziona uno o più file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".xlsx", ".accdb" } },
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/msaccess" } },
                        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx", "com.microsoft.access" } },
                        { DevicePlatform.MacCatalyst, new[] { "com.microsoft.excel.xlsx", "com.microsoft.access" } }
                    })
                };

                var files = await FilePicker.Default.PickMultipleAsync(options);

                if (files is not null && files.Any())
                {
                    var estensioni = allowedExtensions?.Select(e => e.ToLowerInvariant()).ToList();

                    return files
                        .Where(f =>
                            estensioni == null || estensioni.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                        .Select(f => f.FullPath)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore selezione file multipli", ex.Message, "OK");
            }

            return new List<string>();
        }

    }
}
