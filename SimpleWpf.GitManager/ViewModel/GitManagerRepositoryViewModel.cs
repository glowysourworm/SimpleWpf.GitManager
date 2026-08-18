using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerRepositoryViewModel : ViewModelBase
    {
        string _name;
        string _ownerName;
        string _baseDirectory;
        string _workingDirectory;
        string _url;
        DateTimeOffset? _commitLocalWhen;
        DateTimeOffset? _commitRemoteWhen;
        string _commitLocalMessage;
        string _commitRemoteMessage;
        string _commitLocalUser;
        string _commitRemoteUser;
        string _remoteHead;
        long _remoteSize;
        int _behindBy;
        int _aheadBy;
        bool _isAhead;
        bool _isBehind;
        bool _isFork;

        string _loadingMessage;
        bool _isLoading;
        bool _isSelected;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string OwnerName
        {
            get { return _ownerName; }
            set { this.RaiseAndSetIfChanged(ref _ownerName, value); }
        }
        public string BaseDirectory
        {
            get { return _baseDirectory; }
            set { this.RaiseAndSetIfChanged(ref _baseDirectory, value); }
        }
        public string WorkingDirectory
        {
            get { return _workingDirectory; }
            set { this.RaiseAndSetIfChanged(ref _workingDirectory, value); }
        }
        public string Url
        {
            get { return _url; }
            set { this.RaiseAndSetIfChanged(ref _url, value); }
        }
        public DateTimeOffset? CommitLocalWhen
        {
            get { return _commitLocalWhen; }
            set { this.RaiseAndSetIfChanged(ref _commitLocalWhen, value); }
        }
        public DateTimeOffset? CommitRemoteWhen
        {
            get { return _commitRemoteWhen; }
            set { this.RaiseAndSetIfChanged(ref _commitRemoteWhen, value); }
        }
        public string CommitLocalMessage
        {
            get { return _commitLocalMessage; }
            set { this.RaiseAndSetIfChanged(ref _commitLocalMessage, value); }
        }
        public string CommitRemoteMessage
        {
            get { return _commitRemoteMessage; }
            set { this.RaiseAndSetIfChanged(ref _commitRemoteMessage, value); }
        }
        public string CommitLocalUser
        {
            get { return _commitLocalUser; }
            set { this.RaiseAndSetIfChanged(ref _commitLocalUser, value); }
        }
        public string CommitRemoteUser
        {
            get { return _commitRemoteUser; }
            set { this.RaiseAndSetIfChanged(ref _commitRemoteUser, value); }
        }
        public string RemoteHead
        {
            get { return _remoteHead; }
            set { this.RaiseAndSetIfChanged(ref _remoteHead, value); }
        }
        public long RemoteSize
        {
            get { return _remoteSize; }
            set { this.RaiseAndSetIfChanged(ref _remoteSize, value); }
        }
        public int BehindBy
        {
            get { return _behindBy; }
            set { this.RaiseAndSetIfChanged(ref _behindBy, value); }
        }
        public int AheadBy
        {
            get { return _aheadBy; }
            set { this.RaiseAndSetIfChanged(ref _aheadBy, value); }
        }
        public bool IsAhead
        {
            get { return _isAhead; }
            set { this.RaiseAndSetIfChanged(ref _isAhead, value); }
        }
        public bool IsBehind
        {
            get { return _isBehind; }
            set { this.RaiseAndSetIfChanged(ref _isBehind, value); }
        }
        public bool IsFork
        {
            get { return _isFork; }
            set { this.RaiseAndSetIfChanged(ref _isFork, value); }
        }

        public string LoadingMessage
        {
            get { return _loadingMessage; }
            set { this.RaiseAndSetIfChanged(ref _loadingMessage, value); }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set { this.RaiseAndSetIfChanged(ref _isLoading, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        public ObservableCollection<GitManagerLogMessageViewModel> Log { get; set; }

        public GitManagerRepositoryViewModel()
        {
            this.Name = string.Empty;
            this.OwnerName = string.Empty;
            this.BaseDirectory = string.Empty;
            this.WorkingDirectory = string.Empty;
            this.Url = string.Empty;
            this.CommitLocalWhen = DateTime.MinValue;
            this.CommitRemoteWhen = DateTime.MinValue;
            this.CommitLocalMessage = string.Empty;
            this.CommitRemoteMessage = string.Empty;
            this.CommitLocalUser = string.Empty;
            this.CommitRemoteUser = string.Empty;
            this.RemoteHead = string.Empty;
            this.RemoteSize = 0;
            this.BehindBy = 0;
            this.AheadBy = 0;
            this.IsAhead = false;
            this.IsBehind = false;
            this.IsFork = false;

            this.LoadingMessage = string.Empty;
            this.IsLoading = false;
            this.IsSelected = false;

            this.Log = new ObservableCollection<GitManagerLogMessageViewModel>();
        }
    }
}
