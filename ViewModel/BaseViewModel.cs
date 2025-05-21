using CommunityToolkit.Mvvm.ComponentModel;

namespace PvPmo.ViewModel
{
    [INotifyPropertyChanged]
    public partial class BaseViewModel
    {
        [ObservableProperty]
        string tabellaCorrente;        

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        string title = string.Empty;

        public bool IsNotBusy => !IsBusy;

        public void StartProgress()
        {            
            TabellaCorrente = string.Empty;            
        }

        public void ResetProgress()
        {            
            TabellaCorrente = string.Empty;            
        }
    }
}
