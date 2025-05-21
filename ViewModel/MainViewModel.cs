using PvPmo.View;
using PvPmo.UpdateDb;

namespace PvPmo.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            Title = "Home Page";
        }        

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

            var progress = new Progress<string>(label =>
            {                
                TabellaCorrente = label;                
            });
            try
            {
                await InpExlToSql.InpExl("EXL", progress);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore", ex.Message, "OK");
            }

            await Task.Delay(500);
            ResetProgress();

            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnTest()
        {
            if (IsBusy) return;
            IsBusy = true;
            await Shell.Current.DisplayAlert("Test", "Rientro da procedura", "OK");
            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnEnd()
        {
            if (IsBusy) return;
            IsBusy = true;

            var confirm = await Shell.Current.DisplayAlert("Esci", "Vuoi chiudere?", "Si", "No");
            if (confirm)
                Environment.Exit(0);

            IsBusy = false;
        }
    }
}
