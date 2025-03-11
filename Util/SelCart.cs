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
    }
}
