using CommunityToolkit.Mvvm.ComponentModel;

namespace PvPmo.ViewModel
{
    [INotifyPropertyChanged]
    public partial class BaseViewModel
    {
        [ObservableProperty]
        double progressValue;

        [ObservableProperty]
        string tabellaCorrente;

        [ObservableProperty]
        string progressText;

        [ObservableProperty]
        bool isProgressVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        string title = string.Empty;

        public bool IsNotBusy => !IsBusy;

        public void StartProgress()
        {
            ProgressValue = 0;
            ProgressText = "0%";
            TabellaCorrente = string.Empty;
            IsProgressVisible = true;
        }

        public void ResetProgress()
        {
            ProgressValue = 0;
            ProgressText = string.Empty;
            TabellaCorrente = string.Empty;
            IsProgressVisible = false;
        }
    }
}
