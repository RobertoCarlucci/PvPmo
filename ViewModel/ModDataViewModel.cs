namespace PvPmo.ViewModel
{
    public partial class ModDataViewModel : BaseViewModel
    {
        
        public TabDataService _inpData = new();
                        
        public ModDataViewModel()
        {
            Title = "Aggiorna Data File Input";            
        }

        [ObservableProperty]
        bool isRefreshing;

        [RelayCommand]
        async Task BtnAnnullaEsci()
        {
            if (IsBusy)
                return;
            await Shell.Current.GoToAsync("..");
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
