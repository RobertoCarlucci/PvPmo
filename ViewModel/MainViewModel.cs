using PvPmo.View;
using PvPmo.Import;

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
            await Shell.Current.GoToAsync(nameof(GesAcsView));
            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnAgData()
        {
            if (IsBusy) return;
            IsBusy = true;
            await Shell.Current.GoToAsync(nameof(ModDataView));
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
              bool tuttoOk = await InpExlToSql.InpExl("EXL", progress);
                if (tuttoOk == false) 
                {
                    await Shell.Current.DisplayAlert("Errore Aggiornamento Db.",
                    $"La procedura è stata terminata.", "OK");                    
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore Aggiornamento Db.", 
                    $"La procedura è stata terminata controlla il file: {ex.Message}", "OK");
            }

            await Task.Delay(500);
            ResetProgress();

            IsBusy = false;
        }

        [RelayCommand]
        async Task BtnModSetupApp()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            //await Shell.Current.GoToAsync(nameof(JsonEditorView));

            IsBusy = false;
            return;
        }

        //[RelayCommand]
        //async Task BtnExport()
        //{
        //    if (IsBusy) return;
        //    IsBusy = true;
        //    await Shell.Current.DisplayAlert("Test", "Rientro da procedura", "OK");
        //    IsBusy = false;
        //}

        [RelayCommand]
        async Task BtnEnd()
        {
            if (IsBusy) return;
            IsBusy = true;

            var confirm = await Shell.Current.DisplayAlert("Esci", "Vuoi chiudere?", "Si", "No");
            if (confirm)
                System.Diagnostics.Process.GetCurrentProcess().Kill();

            IsBusy = false;
        }
    }
}
