namespace PvPmo.ViewModel
{
    public partial class ModDataViewModel : BaseViewModel
    {
        public ModDataViewModel()
        {
            Title = "Aggiorna Data File Input";
        }

        [RelayCommand]
        Task BtnAnnullaEsci() => Shell.Current.GoToAsync("..");
        
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
