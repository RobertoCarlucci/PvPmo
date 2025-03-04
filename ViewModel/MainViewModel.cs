using PvPmo.Service;
using System.Diagnostics.Eventing.Reader;

namespace PvPmo.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {        
        public MainViewModel() 
        {
            Title = "Test Title";
        }

        [RelayCommand]
        async Task BtnAgDb()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            // Res10Service.ListaRes10();
            

            await Shell.Current.DisplayAlert
                ("Hai premuto BtnAgDb !", $"Non ci posso credere.", "Ok");

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnImpAcs()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await AcsToSql.NewDb();

            await Shell.Current.DisplayAlert
                ("Hai comletato l'importazione dei dati nel Db !", 
                $"Importazione de dati da Access completata .", "Ok");

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnAgData()
        {
            if (IsBusy)
                return;

            IsBusy = true;            

            await Shell.Current.DisplayAlert
                ("Hai premuto BtnAgData !", $"Non ci posso credere.", "Ok");

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
