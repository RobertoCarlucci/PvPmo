using CommunityToolkit.Maui.Storage;

namespace PvPmo.Util
{
    public partial class SelCart : BaseViewModel
    {                
        public static async Task<string> PickFolder()
        {
            try
            {
                var folderResult = await FolderPicker.PickAsync("DCIM");
                if (folderResult.IsSuccessful)
                {
                    var filesCount = Directory.EnumerateFiles(folderResult.Folder.Path).Count();
                    string _path = folderResult.Folder.Path.ToString();
                    return _path;
                }                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert
                    ("Errore Selezione Cartella !", $"Codice: {ex}", "Ok");
            }
            return null;
        }
        public static async Task<String> PickFile()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    if (result.FileName.EndsWith("accdb", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        string ritorno = result.FullPath.ToString();
                        return ritorno;
                    }
                }                
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert
                    ("Errore Selezione File !", $"Codice: {ex}", "Ok");
            }
            return null;
        }
    }
}
