using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerRepositoryViewModel : ViewModelBase
    {
        string _baseDirectory;
        string _gitUrl;
        string _name;
        string _lastCommit;
        bool _isFork;
        uint _size;
        DateTimeOffset _lastAccessLocal;
        DateTimeOffset _lastAccessRemote;

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
        public string LastCommit
        {
            get { return _lastCommit; }
            set { this.RaiseAndSetIfChanged(ref _lastCommit, value); }
        }
        public bool IsFork
        {
            get { return _isFork; }
            set { this.RaiseAndSetIfChanged(ref _isFork, value); }
        }
        public uint Size
        {
            get { return _size; }
            set { this.RaiseAndSetIfChanged(ref _size, value); }
        }
        public DateTimeOffset LastAccessLocal
        {
            get { return _lastAccessLocal; }
            set { this.RaiseAndSetIfChanged(ref _lastAccessLocal, value); }
        }
        public DateTimeOffset LastAccessRemote
        {
            get { return _lastAccessRemote; }
            set { this.RaiseAndSetIfChanged(ref _lastAccessRemote, value); }
        }

        public ObservableCollection<string> Log { get; set; }

        public GitManagerRepositoryViewModel()
        {
            this.BaseDirectory = string.Empty;
            this.GitUrl = string.Empty;
            this.Name = string.Empty;
            this.IsFork = false;
            this.Size = 0;
            this.LastAccessLocal = DateTimeOffset.MinValue;
            this.LastAccessRemote = DateTimeOffset.MinValue;

            this.Log = new ObservableCollection<string>();
        }
    }
}
