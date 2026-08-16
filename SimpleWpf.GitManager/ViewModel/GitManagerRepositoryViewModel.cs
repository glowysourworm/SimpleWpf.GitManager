using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerRepositoryViewModel : ViewModelBase
    {
        string _baseDirectory;
        string _gitUrl;
        string _name;
        string _headName;
        DateTimeOffset _lastCommitRemote;
        DateTimeOffset _lastCommitLocal;
        bool _isFork;
        bool _isAhead;
        bool _isBehind;
        int _commitDelta;
        uint _size;

        string _loadingMessage;
        bool _isLoading;
        bool _isSelected;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string BaseDirectory
        {
            get { return _baseDirectory; }
            set { this.RaiseAndSetIfChanged(ref _baseDirectory, value); }
        }
        public string GitUrl
        {
            get { return _gitUrl; }
            set { this.RaiseAndSetIfChanged(ref _gitUrl, value); }
        }
        public string HeadName
        {
            get { return _headName; }
            set { this.RaiseAndSetIfChanged(ref _headName, value); }
        }
        public DateTimeOffset LastCommitRemote
        {
            get { return _lastCommitRemote; }
            set { this.RaiseAndSetIfChanged(ref _lastCommitRemote, value); }
        }
        public DateTimeOffset LastCommitLocal
        {
            get { return _lastCommitLocal; }
            set { this.RaiseAndSetIfChanged(ref _lastCommitLocal, value); }
        }
        public bool IsFork
        {
            get { return _isFork; }
            set { this.RaiseAndSetIfChanged(ref _isFork, value); }
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
        public int CommitDelta
        {
            get { return _commitDelta; }
            set { this.RaiseAndSetIfChanged(ref _commitDelta, value); }
        }
        public uint Size
        {
            get { return _size; }
            set { this.RaiseAndSetIfChanged(ref _size, value); }
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
            this.BaseDirectory = string.Empty;
            this.GitUrl = string.Empty;
            this.Name = string.Empty;
            this.HeadName = string.Empty;
            this.IsAhead = false;
            this.IsBehind = false;
            this.IsFork = false;
            this.Size = 0;
            this.LastCommitLocal = DateTime.MinValue;
            this.LastCommitRemote = DateTime.MinValue;

            this.LoadingMessage = string.Empty;
            this.IsLoading = false;
            this.IsSelected = false;

            this.Log = new ObservableCollection<GitManagerLogMessageViewModel>();
        }
    }
}
