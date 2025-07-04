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

                if (!result.IsSuccessful || string.IsNullOrWhiteSpace(result.Folder.Path))
                {
                    await Shell.Current.DisplayAlert("Attenzione", "Nessuna cartella selezionata.", "OK");
                    return null;
                }

                var selectedPath = result.Folder.Path;

                if (!Directory.EnumerateFileSystemEntries(selectedPath).Any())
                {
                    await Shell.Current.DisplayAlert("Cartella vuota", "La cartella selezionata non contiene file.", "OK");
                    return null;
                }

                return selectedPath;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore selezione cartella", ex.Message, "OK");
                return null;
            }
        }
        public static async Task<string?> PickFile()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();

                if (result is null)
                {
                    await Shell.Current.DisplayAlert("Annullato", "Nessun file selezionato.", "OK");
                    return null;
                }

                var ext = Path.GetExtension(result.FileName).ToLowerInvariant();

                if (ext is not ".accdb" and not ".xlsx")
                {
                    await Shell.Current.DisplayAlert("Formato non valido", "Seleziona un file .accdb o .xlsx.", "OK");
                    return null;
                }

                var filePath = result.FullPath;

                if (!File.Exists(filePath))
                {
                    await Shell.Current.DisplayAlert("Errore", "Il file selezionato non esiste più sul disco.", "OK");
                    return null;
                }

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                {
                    await Shell.Current.DisplayAlert("File vuoto", "Il file selezionato è vuoto.", "OK");
                    return null;
                }

                return filePath;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore selezione file", ex.Message, "OK");
                return null;
            }
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
                        { DevicePlatform.WinUI, new[] { ".xlsx", ".accdb" } }                
                    })
                };

                var files = await FilePicker.Default.PickMultipleAsync(options);

                if (files == null || !files.Any())
                {
                    await Shell.Current.DisplayAlert("Annullato", "Nessun file selezionato.", "OK");
                    return new List<string>();
                }

                var estensioni = allowedExtensions?.Select(e => e.ToLowerInvariant()).ToList();
                var validi = new List<string>();

                foreach (var f in files)
                {
                    var ext = Path.GetExtension(f.FileName).ToLowerInvariant();

                    if (estensioni != null && !estensioni.Contains(ext))
                        continue;

                    if (!File.Exists(f.FullPath))
                        continue;

                    var info = new FileInfo(f.FullPath);
                    if (info.Length == 0)
                        continue;

                    validi.Add(f.FullPath);
                }

                if (validi.Count == 0)
                {
                    await Shell.Current.DisplayAlert("Nessun file valido", "Tutti i file selezionati sono vuoti, inesistenti o non supportati.", "OK");
                }

                return validi;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore selezione file multipli", ex.Message, "OK");
                return new List<string>();
            }
        }
    }
}
