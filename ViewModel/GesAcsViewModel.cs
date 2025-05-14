using PvPmo.ImportDb;

namespace PvPmo.ViewModel
{
    public partial class GesAcsViewModel : BaseViewModel
    {
        public GesAcsViewModel()
        {
            Title = "Gestione Db Access.";
        }

        [ObservableProperty]
        bool isRefreshing;

        [RelayCommand]
        async Task BtnImpAcs()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await InpAcsToSql.NewDb();            

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnImpSetup()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await InpExlSetup.NewSetup();            

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnExpExl()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            DataTable dt = new DataTable();
            string _exlPath = await SelCart.PickFolder();
            ExportDt.LoadDt(dt);
            //ExportDt.ExportDataSet(dt, _exlPath);
            await Shell.Current.DisplayAlert
                ("Hai esportato il file !",
                $"Esportazione de dati completata .", "Ok");

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
