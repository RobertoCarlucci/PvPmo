namespace PvPmo.ViewModel
{
    public partial class ModDataViewModel : BaseViewModel
    {
        public DataTable _tabData = new DataTable();
        string StrConnSql = Db.Conn.MysqlConn("pmo");
        string QrySql = "SELECT * FROM `01_tabella_data`";
        
        public ModDataViewModel()
        {
            Title = "Aggiorna Data File Input";
            _tabData = SqlSync.SqlQryDataTable(StrConnSql, QrySql, _tabData);
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
