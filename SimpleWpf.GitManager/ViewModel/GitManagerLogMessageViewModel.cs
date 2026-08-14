using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerLogMessageViewModel : ViewModelBase
    {
        string _message;
        DateTime _timestamp;

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { this.RaiseAndSetIfChanged(ref _timestamp, value); }
        }

        public GitManagerLogMessageViewModel()
        {
            this.Message = string.Empty;
            this.Timestamp = DateTime.MinValue;
        }
    }
}
