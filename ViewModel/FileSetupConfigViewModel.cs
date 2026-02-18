using PvPmo.View;
using System.Collections.ObjectModel;

namespace PvPmo.ViewModel;

public partial class FileSetupConfigViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<FileSetupConfig> configurazioni = new();

    public FileSetupConfigViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Editor Configurazione File Setup";
        
        // Carica le configurazioni all'avvio
        Task.Run(async () => await CaricaConfigurazioniAsync());
    }

    private async Task CaricaConfigurazioniAsync()
    {
        try
        {
            IsBusy = true;
            var configs = await _databaseService.GetFileSetupConfigsAsync();
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Configurazioni.Clear();
                foreach (var config in configs)
                {
                    Configurazioni.Add(config);
                }
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", $"Errore nel caricamento: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task AggiungiNuovaConfigurazioneAsync()
    {
        var nuovaConfig = new FileSetupConfig
        {
            FileImp = "nuovo_file.xlsx",
            TabDest = "TabDestinazione",
            Descrizione = "Nuova configurazione"
        };
        
        Configurazioni.Add(nuovaConfig);
    }

    [RelayCommand]
    public async Task SalvaAsync()
    {
        try
        {
            IsBusy = true;
            
            // Salva tutte le configurazioni
            foreach (var config in Configurazioni)
            {
                await _databaseService.SaveFileSetupConfigAsync(config);
            }
            
            await Shell.Current.DisplayAlert("Successo", "Configurazioni salvate con successo!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", $"Errore nel salvataggio: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task BtnAnnullaTorna()
    {
        bool conferma = await Shell.Current.DisplayAlert(
            "Conferma",
            "Vuoi uscire senza salvare le modifiche?", 
            "Sì", 
            "No");
            
        if (conferma)
        {
            await Shell.Current.GoToAsync(nameof(EditorConfigView));

        }
    }

    [RelayCommand]
    public async Task EliminaConfigurazione(FileSetupConfig config)
    {
        bool conferma = await Shell.Current.DisplayAlert(
            "Conferma Eliminazione",
            $"Sei sicuro di voler eliminare la configurazione '{config.FileImp}'?",
            "Sì",
            "No");

        if (conferma)
        {
            try
            {
                if (config.Id > 0)
                {
                    await _databaseService.DeleteFileSetupConfigAsync(config);
                }
                Configurazioni.Remove(config);
                
                await Shell.Current.DisplayAlert("Successo", "Configurazione eliminata!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore", $"Errore nell'eliminazione: {ex.Message}", "OK");
            }
        }
    }
}
