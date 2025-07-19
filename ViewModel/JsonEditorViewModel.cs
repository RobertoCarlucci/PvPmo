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

            switch (tipo)
            {
                case JsonConfigType.Origine:
                    Configurazioni = new(await EmbeddedJsonLoader.LoadJsonAsync<OrigineConfig>(fileName));
                    break;
                case JsonConfigType.Normalizza:
                    Configurazioni = new(await EmbeddedJsonLoader.LoadJsonAsync<NormalizzaConfig>(fileName));
                    break;
                case JsonConfigType.Progress:
                    Configurazioni = new(await EmbeddedJsonLoader.LoadJsonAsync<ProgressConfig>(fileName));
                    break;
                case JsonConfigType.Finalizza:
                    Configurazioni = new(await EmbeddedJsonLoader.LoadJsonAsync<FinalizzaConfig>(fileName));
                    break;
                case JsonConfigType.Utenti:
                    Configurazioni = new(await EmbeddedJsonLoader.LoadJsonAsync<Utente>(fileName));
                    break;
            }
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

        [RelayCommand]
        public async Task ApriEditor(ConfigOption opzione)
        {
            if (opzione is not null)
                await Shell.Current.GoToAsync(nameof(EditorConfigView));
        }
    }
}
