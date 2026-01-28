using PvPmo.Import;
using PvPmo.View;

namespace PvPmo.ViewModel;

public partial class GesAcsViewModel : BaseViewModel
{
    public GesAcsViewModel()
    {
        Title = "Gestione Db Access.";
    }      

    [RelayCommand]
    async Task BtnImpAcs()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        StartProgress();

        var progress = new Progress<string>(label =>
        {
            TabellaCorrente = label;
        });
        try
        {
            await InpAcsToSql.NewDb("ACS", progress);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", ex.Message, "OK");
        }

        await Task.Delay(500);
        ResetProgress();

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
        await Shell.Current.GoToAsync(nameof(MainPageView));
    }
}
