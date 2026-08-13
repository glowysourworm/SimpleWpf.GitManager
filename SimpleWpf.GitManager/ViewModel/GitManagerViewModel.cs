using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerViewModel : ViewModelBase
    {
        string _directory;
        string _user;
        string _password;
        ObservableCollection<GitManagerRepositoryViewModel> _repositories;

        public string Directory
        {
            get { return _directory; }
            set { this.RaiseAndSetIfChanged(ref _directory, value); }
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
        public ObservableCollection<GitManagerRepositoryViewModel> Repositories
        {
            get { return _repositories; }
            set { this.RaiseAndSetIfChanged(ref _repositories, value); }
        }

        public GitManagerViewModel()
        {
            this.Directory = string.Empty;
            this.User = string.Empty;
            this.Password = string.Empty;
            this.Repositories = new ObservableCollection<GitManagerRepositoryViewModel>();
        }
    }
}
