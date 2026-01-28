using PvPmo.View;
using System.Collections.ObjectModel;

namespace PvPmo.ViewModel;

public partial class JsonEditorViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;        

    public JsonEditorViewModel(DatabaseService databaseService)
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
        await Shell.Current.GoToAsync(nameof(JsonEditorView));
    }

    [ObservableProperty]
    private ConfigOption? selectedTipo;

    partial void OnSelectedTipoChanged(ConfigOption? value)
    {
        if (value is not null)
            ApriEditorCommand.Execute(value);
    }
    
    public void CaricaConfigurazioni(ConfigOption tipo)
    {
            
    }


    [RelayCommand]
    public async Task ApriEditor(ConfigOption opzione)
    {           
        
    }
}
