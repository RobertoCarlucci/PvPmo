using PvPmo.Service;

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
                ("Hai premuto BtnImpAcs !", $"Non ci posso credere.", "Ok");

            IsBusy = false;
            return;
        }

        [RelayCommand]
        async Task BtnAgData()
        {
            if (IsBusy)
                return;

            IsBusy = true;

             Db.SqlDb.AddMapping(1, "Test00");
            Db.SqlDb.AddMapping(1, "Test01");
            Db.SqlDb.AddMapping(1, "Test02");
            string StrConn = Db.Conn.MysqlConn("timesheet");
            DataTable tab = new DataTable();
            await Db.SqlDb.SqlBulkCopy("test", StrConn, tab);

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
            if (Conf =! true)
                return;

            IsBusy = false;

            System.Environment.Exit(0);

        }
    }
}
