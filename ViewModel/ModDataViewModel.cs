namespace PvPmo.ViewModel
{
    public partial class ModDataViewModel : BaseViewModel
    {
        public ModDataViewModel()
        {
            Title = "Aggiorna Data File Input";
        }

        [RelayCommand]
        async Task BtnAnnullaEsci()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await Shell.Current.DisplayAlert
                ("Hai premuto BtnAgDb !", $"Non ci posso credere.", "Ok");

            IsBusy = false;
            return;
        }
        [RelayCommand]
        async Task BtnSalvaEsci()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await Shell.Current.DisplayAlert
                ("Hai premuto BtnAgDb !", $"Non ci posso credere.", "Ok");

            IsBusy = false;
            return;
        }

    }
}
