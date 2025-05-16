namespace PvPmo.ViewModel
{
    [INotifyPropertyChanged]
    public partial class BaseViewModel
    {
        // Stato attuale percentuale
        private double _progressValue;
        
        private bool _isProgressVisible;
       
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (SetProperty(ref _progressValue, value))
                {
                    IsProgressVisible = value > 0 && value < 1;                    
                }
            }
        }
        // Mostrare barra
        
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => SetProperty(ref _isProgressVisible, value);
        }

        // Testo formattato tipo "65%"
        
        

        // Utility: resetta tutto
        public void ResetProgress()
        {
            ProgressValue = 0;            
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
