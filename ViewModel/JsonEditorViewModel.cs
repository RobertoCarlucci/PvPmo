using PvPmo.View;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace PvPmo.ViewModel
{
    public partial class JsonEditorViewModel : BaseViewModel
    {
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
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("tipo", out var tipoObj) &&
                tipoObj is string tipoRaw &&
                Enum.TryParse<JsonConfigType>(tipoRaw, out var tipoParsed))
            {
                _ = CaricaAsync(tipoParsed);
            }
        }
        public JsonEditorViewModel()
        {
            Title = "Seleziona Configurazione da Modificare.";
        }
        public async Task CaricaAsync(JsonConfigType tipo)
        {
            string fileName = tipo == JsonConfigType.Utenti ? "utenti.json" : $"{tipo}Config.json";

            TipoSelezionato = TipiDisponibili.FirstOrDefault(c => c.Tipo == tipo);

            Configurazioni.Clear();

            IEnumerable<object>? elementi = tipo switch
            {
                JsonConfigType.Origine => await EmbeddedJsonLoader.LoadJsonAsync<OrigineConfig>(fileName),
                JsonConfigType.Normalizza => await EmbeddedJsonLoader.LoadJsonAsync<NormalizzaConfig>(fileName),
                JsonConfigType.Progress => await EmbeddedJsonLoader.LoadJsonAsync<ProgressConfig>(fileName),
                JsonConfigType.Finalizza => await EmbeddedJsonLoader.LoadJsonAsync<FinalizzaConfig>(fileName),
                JsonConfigType.Utenti => await EmbeddedJsonLoader.LoadJsonAsync<Utente>(fileName),
                _ => Enumerable.Empty<object>()
            };

            foreach (var item in elementi)
                Configurazioni.Add(item);
        }        

        [RelayCommand]
        public async Task SalvaAsync()
        {
            if (TipoSelezionato?.Tipo is not JsonConfigType tipo) return;

            string fileName = tipo == JsonConfigType.Utenti ? "utenti.json" : $"{tipo}Config.json";
            string path = Path.Combine(FileSystem.AppDataDirectory, fileName);
            var json = JsonSerializer.Serialize(Configurazioni, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
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
