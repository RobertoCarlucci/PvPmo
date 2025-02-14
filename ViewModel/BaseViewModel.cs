namespace PvPmo.ViewModel
{
    [INotifyPropertyChanged]
    public partial class BaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        string title = string.Empty;

        public bool IsNotBusy => !isBusy;
    }
}
