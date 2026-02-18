using PvPmo.View;

namespace PvPmo.ViewModel;

public partial class EditorConfigViewModel : BaseViewModel
{
   

    public EditorConfigViewModel()
    {        
        Title = "Seleziona Configurazione da Modificare.";
    }

    [RelayCommand]
    public async Task ApriFileSetupConfig()
    {
        await Shell.Current.GoToAsync(nameof(FileSetupConfigView));
    }

    //[RelayCommand]
    //public async Task SalvaAsync()
    //{
    //    // Implementazione per altre configurazioni se necessario
    //}

    [RelayCommand]
    public async Task BtnAnnullaEsci()
    {
        await Shell.Current.GoToAsync(nameof(MainPageView));
    }

    //[RelayCommand]
    //public async Task BtnAnnullaTorna()
    //{
    //    await Shell.Current.GoToAsync("..");
    //}
}
