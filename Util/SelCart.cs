using CommunityToolkit.Maui.Storage;

namespace PvPmo.Util
{
    public partial class SelCart : BaseViewModel
    {                
        public static async Task<string> PickFolderStatic(CancellationToken cancellationToken)
        {
            var folderResult = await FolderPicker.PickAsync("DCIM", cancellationToken);
            if (folderResult.IsSuccessful)
            {
                var filesCount = Directory.EnumerateFiles(folderResult.Folder.Path).Count();
                string _path = folderResult.Folder.Path.ToString();
                return _path;                
            }
            else
            {
                return "";                
            }
        }
        public static async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await result.OpenReadAsync();
                        var image = ImageSource.FromStream(() => stream);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }
    }
}
