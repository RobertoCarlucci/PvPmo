using PvPmo.View;
using System.Collections.ObjectModel;

namespace PvPmo.ViewModel
{
    public partial class JsonEditorViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ConfigOption? tipoSelezionato;

        [ObservableProperty]
        private ObservableCollection<object> configurazioni = new();
        
        [ObservableProperty]
        private ObservableCollection<ConfigOption> tipiDisponibili = new()
            {
                new ConfigOption { Tipo = JsonConfigType.Normalizza },
                new ConfigOption { Tipo = JsonConfigType.Origine },
                new ConfigOption { Tipo = JsonConfigType.Progress },
                new ConfigOption { Tipo = JsonConfigType.Finalizza },
                new ConfigOption { Tipo = JsonConfigType.Utenti }
            };

        public JsonEditorViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Seleziona Configurazione da Modificare.";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("tipo", out var tipoObj) &&
                tipoObj is string tipoRaw &&
                Enum.TryParse<JsonConfigType>(tipoRaw, out var tipoParsed))
            {
                _ = CaricaAsync(tipoParsed);
            }
        }

        public async Task CaricaAsync(JsonConfigType tipo)
        {
            TipoSelezionato = TipiDisponibili.FirstOrDefault(c => c.Tipo == tipo);

            Configurazioni.Clear();

            IEnumerable<object>? elementi = tipo switch
            {
                JsonConfigType.Origine => await _databaseService.GetOrigineConfigsAsync(),
                JsonConfigType.Normalizza => await _databaseService.GetNormalizzaConfigsAsync(),
                JsonConfigType.Progress => await _databaseService.GetProgressConfigsAsync(),
                JsonConfigType.Finalizza => await _databaseService.GetFinalizzaConfigsAsync(),
                JsonConfigType.Utenti => await _databaseService.GetUtentiAsync(),
                _ => Enumerable.Empty<object>()
            };

            foreach (var item in elementi)
                Configurazioni.Add(item);
        }        

        [RelayCommand]
        public async Task SalvaAsync()
        {
            if (TipoSelezionato?.Tipo is not JsonConfigType tipo) return;

            try
            {
                foreach (var item in Configurazioni)
                {
                    switch (tipo)
                    {
                        case JsonConfigType.Utenti:
                            await _databaseService.SaveUtenteAsync((Utente)item);
                            break;
                        case JsonConfigType.Origine:
                            await _databaseService.SaveOrigineConfigAsync((OrigineConfig)item);
                            break;
                        case JsonConfigType.Normalizza:
                            await _databaseService.SaveNormalizzaConfigAsync((NormalizzaConfig)item);
                            break;
                        case JsonConfigType.Progress:
                            await _databaseService.SaveProgressConfigAsync((ProgressConfig)item);
                            break;
                        case JsonConfigType.Finalizza:
                            await _databaseService.SaveFinalizzaConfigAsync((FinalizzaConfig)item);
                            break;
                    }
                }

                await Shell.Current.DisplayAlert("Successo", "Modifiche salvate nel database!", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore", $"Errore durante il salvataggio: {ex.Message}", "OK");
            }
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
            Configurazioni.Clear();

            object? nuovo = tipo.Tipo switch
            {
                JsonConfigType.Utenti => new Utente
                {
                    NomeUtente = "",
                    PasswordHash = "",
                    Ruolo = "",
                    Tipo = TipoUtente.Locale
                },
                JsonConfigType.Origine => new OrigineConfig
                {
                    DbInp = "",
                    Tabella = "",
                    DbDest = "",
                    TabellaSql = "",
                    WorkSheet = "",
                    InpType = ""
                },
                JsonConfigType.Progress => new ProgressConfig
                {
                    TabellaSql = "",
                    DbName = "",
                    Descrizione = ""
                },
                JsonConfigType.Normalizza => new NormalizzaConfig
                {
                    Azione = "",
                    ColDaMod = "",
                    Modifica = "",
                    TipoCol = "",
                    TabellaMod = "",
                    DbDest = "",
                    InpType = ""
                },
                JsonConfigType.Finalizza => new FinalizzaConfig
                {
                    Azione = "",
                    TabConfronto = "",
                    ColConfronto = "",
                    TabTestare = "",
                    ColDaTestare = "",
                    DbTabConfronto = "",
                    DbTabTest = ""
                },
                _ => null
            };
            if (nuovo is not null)
                Configurazioni.Add(nuovo);           
        }


        [RelayCommand]
        public async Task ApriEditor(ConfigOption opzione)
        {           
            TipoSelezionato = opzione;
            await CaricaAsync(opzione.Tipo);            

            if (opzione is not null)
                await Shell.Current.GoToAsync(nameof(EditorConfigView));
        }
    }
}
