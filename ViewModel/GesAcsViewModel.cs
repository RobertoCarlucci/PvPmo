using PvPmo.ImportDb;

namespace PvPmo.ViewModel
{
    public partial class GesAcsViewModel : BaseViewModel
    {
        public GesAcsViewModel()
        {
            Title = "Gestione Db Access.";
        }

        private bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }

        [RelayCommand]
        async Task BtnImpAcs()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await InpAcsToSql.NewDb();

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnImpSetup()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await InpExlSetup.NewSetup();

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnApriLog()
        {
            try
            {
                var latestLog = LogManager.GetLatestLogFilePath();

                if (!string.IsNullOrEmpty(latestLog) && File.Exists(latestLog))
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest("Visualizza Log", new ReadOnlyFile(latestLog)));
                }
                else
                {
                    await Shell.Current.DisplayAlert("Log", "Nessun file di log trovato.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore apertura log", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task BtnAnnullaEsci()
        {
            if (IsBusy)
                return;
            await Shell.Current.GoToAsync("..");
        }
    }
}
