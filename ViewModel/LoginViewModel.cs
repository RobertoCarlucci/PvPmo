using PvPmo.View;

namespace PvPmo.ViewModel;
public partial class LoginViewModel : BaseViewModel
{
    private readonly ServizioAutenticazione _auth;
    private readonly string utenteWindows;

    public LoginViewModel()
    {
        _auth = new ServizioAutenticazione();
        utenteWindows = $"{Environment.UserDomainName}\\{Environment.UserName}";
        Title = "Pagina di Accesso";
    }

    [ObservableProperty] private string nomeUtente = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string messaggioErrore = string.Empty;
    [ObservableProperty] private bool mostraErrore = false;

    public string EtichettaWindowsLogin => $"Accedi come: {utenteWindows}";

    [RelayCommand]
    private async Task Accedi()
    {
        IsBusy = true;
        try
        {
            var utente = _auth.Autentica(NomeUtente, Password);
            if (utente is not null)
                await Shell.Current.GoToAsync(nameof(MainPage));
            //App.Current.MainPage = new MainPage(new MainViewModel(utente));
            else
                MostraErrore = (MessaggioErrore = "Credenziali non valide.") != null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AccediWindows()
    {
        IsBusy = true;
        try
        {
            var utente = _auth.Autentica(utenteWindows, null);
            if (utente is not null)
                await Shell.Current.GoToAsync(nameof(MainPage));
            //App.Current.MainPage = new MainPage(new MainViewModel(utente));
            else
                MostraErrore = (MessaggioErrore = $"L'utente \"{utenteWindows}\" non è autorizzato.") != null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Riprova()
    {
        NomeUtente = string.Empty;
        Password = string.Empty;
        MostraErrore = false;
        MessaggioErrore = string.Empty;
    }

    [RelayCommand]
    private void Esci()
    {
        // Esci immediatamente
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}
