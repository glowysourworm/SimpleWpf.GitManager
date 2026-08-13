using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerRepositoryViewModel : ViewModelBase
    {
        string _baseDirectory;
        string _gitUrl;
        string _name;
        string _user;
        string _password;
        bool _isFork;
        uint _size;
        DateTime _lastAccessLocal;
        DateTime _lastAccessRemote;

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
        public string User
        {
            get { return _user; }
            set { this.RaiseAndSetIfChanged(ref _user, value); }
        }
        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(ref _password, value); }
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
        public DateTime LastAccessLocal
        {
            get { return _lastAccessLocal; }
            set { this.RaiseAndSetIfChanged(ref _lastAccessLocal, value); }
        }
        public DateTime LastAccessRemote
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
            this.User = string.Empty;
            this.Password = string.Empty;
            this.IsFork = false;
            this.Size = 0;
            this.LastAccessLocal = DateTime.MinValue;
            this.LastAccessRemote = DateTime.MinValue;

            this.Log = new ObservableCollection<string>();
        }
    }
}
