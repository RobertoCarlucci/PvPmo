using CommunityToolkit.Mvvm.ComponentModel;

namespace PvPmo.ViewModel
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        string tabellaCorrente = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        // Replacing the field with a partial property for AOT compatibility
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        private string title = string.Empty;
        
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
