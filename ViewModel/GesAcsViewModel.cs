using PvPmo.Service;

namespace PvPmo.ViewModel
{
    public partial class GesAcsViewModel : BaseViewModel
    {
        public GesAcsViewModel()
        {
            Title = "Gestione import Access.";
        }

        [ObservableProperty]
        bool isRefreshing;

        [RelayCommand]
        async Task BtnImpAcs()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await AcsToSql.NewDb();

            await Shell.Current.DisplayAlert
                ("Hai completato l'importazione dei dati nel Db !",
                $"Importazione de dati da Access completata .", "Ok");

            IsBusy = false;
            return;
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
