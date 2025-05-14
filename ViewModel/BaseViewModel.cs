namespace PvPmo.ViewModel
{
    [INotifyPropertyChanged]
    public partial class BaseViewModel
    {
        // Stato attuale percentuale
        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (SetProperty(ref _progressValue, value))
                {
                    IsProgressVisible = value > 0 && value < 1;
                    ProgressText = $"{value:P0}";
                }
            }
        }

        // Mostrare barra
        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value);
        }

        // Testo formattato tipo "65%"
        private string _progressText = string.Empty;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        // Utility: resetta tutto
        public void ResetProgress()
        {
            ProgressValue = 0;
            ProgressText = string.Empty;
            IsProgressVisible = false;
        }

        // Utility: avvia visibilità
        public void StartProgress()
        {
            ProgressValue = 0;
            IsProgressVisible = true;
        }

        [ObservableProperty]

        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        string title = string.Empty;

        public bool IsNotBusy => !isBusy;
    }
}
