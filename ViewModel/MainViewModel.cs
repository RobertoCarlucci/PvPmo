using PvPmo.View;
using System;

namespace PvPmo.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {        
        public MainViewModel() 
        {
            Title = "Home Page";
        }

        [ObservableProperty]
        bool isRefreshing;

        [RelayCommand]
        async Task BtnGesAcs()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await Shell.Current.GoToAsync(nameof(GesAcs));

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnAgData() 
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await Shell.Current.GoToAsync(nameof(ModData));

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnAgDb()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await UpdateDb.ExltoSql.InpExl();

            await Shell.Current.DisplayAlert
                ("Hai premuto BtnAgDb !", $"Non ci posso credere.", "Ok");

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnEnd()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            var Conf = await Shell.Current.DisplayAlert
                ("Chiudi ed Esci.", "Vuoi chiudere l'aplicazione ?", "Si", "No");
            if (Conf == true)
                Environment.Exit(0);
            else
                IsBusy = false;            
            return;
        }
    }
}
