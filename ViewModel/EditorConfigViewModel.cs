using PvPmo.View;

namespace PvPmo.ViewModel;

public partial class EditorConfigViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;        

    public EditorConfigViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Seleziona Configurazione da Modificare.";
    }            

    [RelayCommand]
    public async Task SalvaAsync()
    {
        
    }

    [RelayCommand]
    public async Task BtnAnnullaEsci()
    {
        await Shell.Current.GoToAsync(nameof(MainPageView));
    }

    [RelayCommand]
    public async Task BtnAnnullaTorna()
    {
        await Shell.Current.GoToAsync(nameof(MainPageView));
    }
    
}
