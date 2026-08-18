using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerLoadingViewModel : ViewModelBase
    {
        bool _loading;
        bool _showProgress;
        int _progressPercent;
        string _progressMessage;

        public bool Loading
        {
            get { return _loading; }
            set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }
        public bool ShowProgress
        {
            get { return _showProgress; }
            set { this.RaiseAndSetIfChanged(ref _showProgress, value); }
        }
        public int ProgressPercent
        {
            get { return _progressPercent; }
            set { this.RaiseAndSetIfChanged(ref _progressPercent, value); }
        }
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { this.RaiseAndSetIfChanged(ref _progressMessage, value); }
        }

        public GitManagerLoadingViewModel()
        {
            this.Loading = false;
            this.ShowProgress = false;
            this.ProgressMessage = string.Empty;
            this.ProgressPercent = 0;
        }
    }
}
