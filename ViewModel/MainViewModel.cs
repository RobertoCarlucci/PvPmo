using PvPmo.View;
using PvPmo.UpdateDb;

namespace PvPmo.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            Title = "Home Page";
            ProgressValue = 0.5;
            IsProgressVisible = true;
        }

        [ObservableProperty]
        bool isRefreshing;

        [RelayCommand]
        async Task BtnGesAcs()
        {
            if (IsBusy) return;

            IsBusy = true;
            await Shell.Current.GoToAsync(nameof(GesAcs));
            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnAgData()
        {
            if (IsBusy) return;

            IsBusy = true;
            await Shell.Current.GoToAsync(nameof(ModData));
            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnAgDb()
        {
            if (IsBusy) return;

            IsBusy = true;
            StartProgress();

            var progress = new Progress<double>(value =>
            {
                ProgressValue = value;
            });

            await InpExlToSql.InpExl(progress); // passa la progress

            ResetProgress();
            IsBusy = false;
        }


        [RelayCommand]
        async Task BtnTest()
        {
            if (IsBusy) return;

            IsBusy = true;

            await Shell.Current.DisplayAlert("Fine Test!", "Rientro da procedura.", "OK");

            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnEnd()
        {
            if (IsBusy) return;

            IsBusy = true;

            var confirm = await Shell.Current.DisplayAlert(
                "Chiudi ed Esci", "Vuoi chiudere l'applicazione?", "Si", "No");

            if (confirm)
            {
                Environment.Exit(0);
            }
            else
            {
                IsBusy = false;
            }
        }
    }
}
