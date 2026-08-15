using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerRepositoryViewModel : ViewModelBase
    {
        string _baseDirectory;
        string _gitUrl;
        string _name;
        string _lastCommitRemote;
        string _lastCommitLocal;
        bool _isFork;
        bool _isHeadUpToDate;
        uint _size;
        DateTimeOffset _lastFetch;

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
        public string LastCommitRemote
        {
            get { return _lastCommitRemote; }
            set { this.RaiseAndSetIfChanged(ref _lastCommitRemote, value); }
        }
        public string LastCommitLocal
        {
            get { return _lastCommitLocal; }
            set { this.RaiseAndSetIfChanged(ref _lastCommitLocal, value); }
        }
        public bool IsFork
        {
            get { return _isFork; }
            set { this.RaiseAndSetIfChanged(ref _isFork, value); }
        }
        public bool IsHeadUpToDate
        {
            get { return _isHeadUpToDate; }
            set { this.RaiseAndSetIfChanged(ref _isHeadUpToDate, value); }
        }
        public uint Size
        {
            get { return _size; }
            set { this.RaiseAndSetIfChanged(ref _size, value); }
        }
        public DateTimeOffset LastFetch
        {
            get { return _lastFetch; }
            set { this.RaiseAndSetIfChanged(ref _lastFetch, value); }
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
            this.IsFork = false;
            this.IsHeadUpToDate = false;
            this.Size = 0;
            this.LastCommitLocal = string.Empty;
            this.LastCommitRemote = string.Empty;
            this.LastFetch = DateTimeOffset.MinValue;

            this.LoadingMessage = string.Empty;
            this.IsLoading = false;
            this.IsSelected = false;

            this.Log = new ObservableCollection<GitManagerLogMessageViewModel>();
        }
    }
}
